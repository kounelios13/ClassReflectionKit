using System.Text;
using System.Text.RegularExpressions;
using ClassReflectionKit.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassReflectionKit.Extensions;

public static class ClassReflectionKitExtensions
{
    public static string GetClassName(this ClassDeclarationSyntax cl) 
    {
        return cl.Identifier.ValueText;
    }

    public static string GetPropName(this PropertyDeclarationSyntax prop)
    {
        return prop.Identifier.ValueText;
    }

    public static string GetNSName(this NamespaceDeclarationSyntax ns)
    {
        return ns.Name.ToFullString();
    }
    // Get the simplified type name of the property 
    // e.g. if type is string[] or IEnumerable<string> return string
    public static string GetSimplifiedPropTypeName(this PropertyDeclarationSyntax prop)
    {
        if (!prop.IsArrayLike()) return prop.Type.ToFullString().Trim();
        var name = prop.Type.ToFullString().Trim();
        if (name.EndsWith("[]"))
        {
            return name.Substring(0, name.Length - 2);
        }
        // IEnumerable case
        var pattern = @"<([^>]*)>";

        var match = Regex.Match(name, pattern);
        return match.Success ? match.Groups[1].Value : "";
    }

    public static bool IsArrayLike(this PropertyDeclarationSyntax prop)
    {
        var propTypeStr = prop.Type.ToFullString().TrimEnd();
        return propTypeStr.EndsWith("[]") || propTypeStr.StartsWith("List<") || propTypeStr.StartsWith("IEnumerable");
    }

    public static string ToTemplateString(this TemplateClassInfo templateInfo)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"#region {templateInfo.ClassName}");
        sb.AppendLine($"public class {templateInfo.ClassName} " + "{");
        foreach (var p in templateInfo.ClassProperties)
        {
            sb.AppendLine($"\t{p}");
        }
        sb.AppendLine("}");
        sb.AppendLine($"#endregion {templateInfo.ClassName}");
        return sb.ToString();
    }

    public static string ToTemplateString(this ClassPropertyInfo propertyInfo)
    {
        var postfix = "{ get; set; }";
        if (propertyInfo.IsArray)
        {
            return $"public IEnumerable<{propertyInfo.PropTypeName}> {propertyInfo.PropName} {postfix}";
        }
        return $"public {propertyInfo.PropTypeName}{(propertyInfo.IsNullable ? "?" : "")} {propertyInfo.PropName} {postfix}";
    }
}
