# ClassReflectionKit

ClassReflectionKit is a small .NET 8 library for parsing C# source code and turning class declarations into simple models that can be inspected, transformed, or rendered into generated code.

It uses the Roslyn C# parser (`Microsoft.CodeAnalysis.CSharp`). Despite its name, it does not load assemblies and does not use runtime reflection. The input is C# source text or paths to C# files.

## What It Does

The library can:

- Parse one or more C# source snippets or files.
- Group parsed syntax trees by namespace.
- List namespaces found in the input.
- Find a class by namespace and name.
- Read public or non-public property declarations discovered in a class syntax tree.
- Describe each property using its name, simplified type name, nullability, and collection status.
- Apply custom transformations to one class or to all classes in a namespace.
- Render a `TemplateClassInfo` as a basic C# class template.

The parser works on syntax. It does not compile the input, resolve symbols, evaluate attributes, or infer types from referenced projects.

## Requirements

- .NET 8.0 or later
- C# source using syntax supported by the referenced Roslyn version

## Installation

Add the NuGet package to a .NET 8 project:

```bash
dotnet add package ClassReflectionKit --version 1.0.6
```

Or add the package reference directly to the project file:

```xml
<PackageReference Include="ClassReflectionKit" Version="1.0.6" />
```

The package includes the `ClassReflectionKit.Helpers`, `ClassReflectionKit.Models`, and `ClassReflectionKit.Extensions` namespaces.

## Quick Start

The following example parses source text, lists the namespaces it found, reads the classes in one namespace, and renders each class as a template:

```csharp
using ClassReflectionKit.Extensions;
using ClassReflectionKit.Helpers;

var source = """
	namespace Example.Models;

	public class Customer
	{
		public string Name { get; set; } = string.Empty;
		public int[] OrderIds { get; set; } = [];
	}
	""";

var helper = new ClassReflectionKitHelper();
helper.InitializeFromCodeInputs(source);

foreach (var ns in helper.GetNameSpaces())
{
	Console.WriteLine($"Namespace: {ns}");

	foreach (var classInfo in helper.GetNSClasses(ns))
	{
		Console.WriteLine(classInfo.ToTemplateString());
	}
}
```

This produces output similar to:

```csharp
// #region Customer
public class Customer {
	public string Name { get; set; }
	public IEnumerable<int> OrderIds { get; set; }
}
// #endregion Customer
```

`ToTemplateString()` is intentionally a basic renderer. It does not emit namespaces, using directives, constructors, initializers, attributes, access modifiers from the input, or the original formatting.

## Initialization

### From source text

Use `InitializeFromCodeInputs` when source is already available in memory:

```csharp
var helper = new ClassReflectionKitHelper();

helper.InitializeFromCodeInputs(
	"""
	namespace Example;
	public class First { public string Value { get; set; } = string.Empty; }
	""",
	"""
	namespace Example;
	public class Second { public int Count { get; set; } }
	"""
);
```

Each source item is parsed as a separate syntax tree. Multiple items may contribute classes to the same namespace.

### From file paths

Use `InitializeFromFilePaths` to read files from disk:

```csharp
var helper = new ClassReflectionKitHelper();

helper.InitializeFromFilePaths(
	"Models/Customer.cs",
	"Models/Order.cs"
);
```

Paths are passed to `File.ReadAllText`, so normal relative-path and absolute-path rules apply. File read errors are not caught by the library and are propagated to the caller.

### Initialization behavior

- A new helper starts with an empty collection of syntax trees.
- Calling either initialization method again adds to the existing collection; there is no reset or clear method.
- A file or snippet contributes only the first namespace declaration found in its syntax tree.
- Inputs without a namespace are ignored by `InitializeFromFilePaths`.
- `InitializeFromCodeInputs` stops processing the remaining inputs when it encounters a source item without a namespace.
- Parsing does not perform a compilation. A syntactically invalid source item can still be stored; use `IsCodeSnippetValid` when validation is required before initialization.

## Reading Parsed Classes

### `GetNameSpaces`

```csharp
string[] namespaces = helper.GetNameSpaces();
```

Returns the namespaces currently represented by the helper. The method name is part of the public API and is spelled `GetNameSpaces`.

### `GetClassInfo`

```csharp
var customer = helper.GetClassInfo("Example.Models", "Customer");

if (customer is not null)
{
	Console.WriteLine(customer.ClassName);
	Console.WriteLine(customer.NameSpace);

	foreach (var property in customer.ClassProperties)
	{
		Console.WriteLine($"{property.PropName}: {property.PropTypeName}");
	}
}
```

The method returns `null` when the namespace or class cannot be found. A class name is matched against the class declaration identifier within the requested namespace.

### `GetNSClasses`

```csharp
List<TemplateClassInfo> classes = helper.GetNSClasses("Example.Models");
```

Returns an empty list when the namespace is not known. Classes are collected from all syntax trees stored for that namespace.

## Models

### `TemplateClassInfo`

`TemplateClassInfo` describes one class:

| Member | Type | Meaning |
| --- | --- | --- |
| `ClassName` | `string` | Name of the class declaration. |
| `NameSpace` | `string` | Namespace used to index the syntax tree. |
| `ClassProperties` | `List<ClassPropertyInfo>` | Properties found in the class. |
| `MetaData` | `List<MetaDataInfo>` | Caller-defined metadata attached to the class. |

### `ClassPropertyInfo`

| Member | Type | Meaning |
| --- | --- | --- |
| `PropName` | `string` | Name of the property declaration. |
| `PropTypeName` | `string` | Type name after collection simplification. |
| `IsNullable` | `bool` | Whether the source type text contains `?`. |
| `IsArray` | `bool` | Whether the property is recognized as an array-like collection. |
| `IsCustomClass` | `bool` | Caller-controlled flag; the library does not set it automatically. |
| `MetaData` | `List<MetaDataInfo>` | Caller-defined metadata attached to the property. |

### `MetaDataInfo`

`MetaDataInfo` contains a caller-defined `Key` and `Value`. Metadata is available for custom processing, but the built-in renderer does not output it.

## Property Recognition

The parser creates `ClassPropertyInfo` objects from property declaration syntax. The built-in collection detection recognizes:

| Source type | `IsArray` | `PropTypeName` |
| --- | ---: | --- |
| `string` | `false` | `string` |
| `string?` | `false` | `string?` |
| `int[]` | `true` | `int` |
| `List<Order>` | `true` | `Order` |
| `IEnumerable<Order>` | `true` | `Order` |

For recognized array-like types, `ToTemplateString()` emits `IEnumerable<T>`. For other types it emits the simplified source type and appends `?` when `IsNullable` is true.

Detection is intentionally limited to the syntax patterns implemented by `IsArrayLike`: types ending in `[]`, types beginning with `List<`, and types beginning with `IEnumerable`. Other collection types, aliases, nested generic types, and nullable collection spellings may require a custom transformation or a subclass.

The current implementation visits property declarations beneath a class syntax node. It is therefore syntax-oriented and should not be treated as a complete C# semantic model.

## Custom Processing

Custom delegates let callers modify individual class models or the complete namespace result.

### Transform one class

`ProcessClassInfo` receives a `TemplateClassInfo?` and returns a `TemplateClassInfo?`:

```csharp
static ProcessClassInfo RemoveGeneratedFlags()
{
	return classInfo =>
	{
		if (classInfo is null)
		{
			return null;
		}

		classInfo.ClassProperties = classInfo.ClassProperties
			.Where(property => !property.PropName.EndsWith("Specified"))
			.ToList();

		return classInfo;
	};
}

var customer = helper.GetClassInfo(
	"Example.Models",
	"Customer",
	RemoveGeneratedFlags()
);
```

The model is mutable. A delegate may update properties, flags, and metadata in place or return a replacement model. Returning `null` is allowed by the delegate type; callers should handle that possibility.

### Transform all classes in a namespace

`ProcessNSClasses` receives the list of class models and returns a list:

```csharp
static ProcessNSClasses SortClasses()
{
	return classes => classes
		.OrderBy(classInfo => classInfo.ClassName)
		.ToList();
}

var classes = helper.GetNSClasses("Example.Models", SortClasses());
```

### Apply both transformations

The three-argument overload applies the class delegate to each class first, then applies the namespace delegate to the resulting list:

```csharp
var classes = helper.GetNSClasses(
	"Example.Models",
	classes => classes.OrderBy(x => x.ClassName).ToList(),
	classInfo =>
	{
		foreach (var property in classInfo!.ClassProperties)
		{
			property.MetaData.Add(new MetaDataInfo
			{
				Key = "source",
				Value = classInfo.ClassName
			});
		}

		return classInfo;
	});
```

## Inheritance and Custom Helpers

`ClassReflectionKitHelper` exposes virtual `GetClassInfo` and `GetNSClasses` methods, so a project can package recurring transformations in a derived helper:

```csharp
public sealed class DomainClassHelper : ClassReflectionKitHelper
{
	public override TemplateClassInfo? GetClassInfo(string ns, string className)
	{
		var classInfo = base.GetClassInfo(ns, className);
		if (classInfo is null)
		{
			return null;
		}

		foreach (var property in classInfo.ClassProperties)
		{
			property.MetaData.Add(new MetaDataInfo
			{
				Key = "domain",
				Value = "true"
			});
		}

		return classInfo;
	}
}
```

Overriding `GetClassInfo` also affects the default `GetNSClasses(string ns)` path, because namespace processing obtains each class through the virtual class method. Keep overrides small and preserve the expected nullable return behavior.

## Validating Source

The interface exposes `IsCodeSnippetValid`:

```csharp
IClassReflectionKitHelper helper = new ClassReflectionKitHelper();

bool valid = helper.IsCodeSnippetValid("namespace Example; public class Customer { }");
```

The method parses the text and returns `false` when Roslyn reports a diagnostic with error severity. It validates syntax only; it does not verify references, accessibility, type resolution, or project compilation.

This member is implemented explicitly on `ClassReflectionKitHelper`, so call it through `IClassReflectionKitHelper` as shown above. It is not directly callable from a variable statically typed as `ClassReflectionKitHelper`.

## Complete Example

This example combines source loading, namespace processing, property transformation, and rendering:

```csharp
using ClassReflectionKit.Extensions;
using ClassReflectionKit.Helpers;
using ClassReflectionKit.Models;

var helper = new ClassReflectionKitHelper();
helper.InitializeFromFilePaths(
	"Samples/Address.cs",
	"Samples/User.cs",
	"Samples/Employee.cs"
);

static ProcessClassInfo HandleSpecifiedProperties()
{
	return classInfo =>
	{
		if (classInfo is null)
		{
			return null;
		}

		var specifiedNames = classInfo.ClassProperties
			.Where(property => property.PropName.EndsWith("Specified"))
			.Select(property => property.PropName)
			.ToHashSet();

		foreach (var specifiedName in specifiedNames)
		{
			var valueName = specifiedName[..^"Specified".Length];
			var valueProperty = classInfo.ClassProperties
				.FirstOrDefault(property => property.PropName == valueName);

			if (valueProperty is not null)
			{
				valueProperty.IsNullable = true;
			}
		}

		classInfo.ClassProperties = classInfo.ClassProperties
			.Where(property => !specifiedNames.Contains(property.PropName))
			.ToList();

		return classInfo;
	};
}

static ProcessNSClasses MarkCustomClasses()
{
	return classes =>
	{
		var classNames = classes
			.Select(classInfo => classInfo.ClassName)
			.ToHashSet();

		foreach (var classInfo in classes)
		{
			foreach (var property in classInfo.ClassProperties)
			{
				property.IsCustomClass = classNames.Contains(property.PropTypeName)
					&& property.PropTypeName != classInfo.ClassName;
			}
		}

		return classes;
	};
}

foreach (var ns in helper.GetNameSpaces())
{
	var classes = helper.GetNSClasses(
		ns,
		MarkCustomClasses(),
		HandleSpecifiedProperties()
	);

	foreach (var classInfo in classes)
	{
		Console.WriteLine(classInfo.ToTemplateString());
	}
}
```

## API Reference

### `IClassReflectionKitHelper`

```csharp
TemplateClassInfo? GetClassInfo(string ns, string className);
TemplateClassInfo? GetClassInfo(
	string ns,
	string className,
	ProcessClassInfo procDelegate
);
string[] GetNameSpaces();
List<TemplateClassInfo> GetNSClasses(string ns);
List<TemplateClassInfo> GetNSClasses(
	string ns,
	ProcessNSClasses procDelegate
);
List<TemplateClassInfo> GetNSClasses(
	string ns,
	ProcessNSClasses procDelegate,
	ProcessClassInfo procClassDelegate
);
void InitializeFromCodeInputs(params string[] codeItems);
void InitializeFromFilePaths(params string[] filePaths);
bool IsCodeSnippetValid(string codeSnippet);
```

### Extension methods

The `ClassReflectionKit.Extensions` namespace contains these public extensions:

- `GetClassName(ClassDeclarationSyntax)`: returns the class identifier.
- `GetPropName(PropertyDeclarationSyntax)`: returns the property identifier.
- `GetNSName(NamespaceDeclarationSyntax)` and `GetNSName(BaseNamespaceDeclarationSyntax)`: returns the namespace name without line breaks.
- `GetSimplifiedPropTypeName(PropertyDeclarationSyntax)`: removes `[]` or extracts the single generic argument for recognized collection syntax.
- `IsArrayLike(PropertyDeclarationSyntax)`: identifies `[]`, `List<T>`, and `IEnumerable<T>` syntax.
- `ToTemplateString(TemplateClassInfo)`: renders a class model.
- `ToTemplateString(ClassPropertyInfo)`: renders one property model.

## Limitations and Considerations

- The library parses source text rather than compiled types.
- It uses the first namespace declaration found in each input. Files containing multiple namespaces should be split if each namespace must be indexed independently.
- Global-namespace files are not indexed because no namespace name is available.
- There is no public method to remove previously initialized syntax trees or reset a helper. Create a new helper when a fresh parse set is needed.
- The parser does not report a structured error result from initialization. Validate snippets separately when input quality matters.
- Type names are preserved as source text except for the collection simplification described above.
- `IsNullable` is based on the presence of `?` in the property type text, not nullable flow analysis.
- `IsCustomClass` and metadata are extension points for callers; they are not inferred or rendered automatically.
- The generated template always uses `IEnumerable<T>` for recognized collections, but it does not add a `using System.Collections.Generic;` directive.
- The helper stores mutable syntax trees and model data in ordinary collections and is not documented as thread-safe.

## Building from Source

From the repository root:

```bash
dotnet build ClassReflectionKit/ClassReflectionKit.csproj
```

To run the included demonstration project:

```bash
dotnet run --project Demo/ClassReflectionUtilLib.csproj
```

The demo reads the sample classes under `Demo/Samples`, applies custom delegates, and prints generated templates.

## License

This project is licensed under the MIT License. See [LICENSE.txt](../LICENSE.txt).