using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ClassReflectionKit.Extensions;
using ClassReflectionKit.Models;
namespace ClassReflectionKit.Helpers;

public class ClassReflectionKitHelper
{
    private Dictionary<string, List<SyntaxTree>> NamespaceSyntaxTrees { get; set; } = new();

    private SyntaxTree GetFileSyntaxTree(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
        return syntaxTree;
    }

    public void InitializeFromFilePaths(params string[] filePaths)
    {
        foreach (var filePath in filePaths)
        {
            var syntaxTree = GetFileSyntaxTree(filePath);
            var root = syntaxTree.GetRoot();
            var ns = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.GetNSName();
            if (ns == null) continue;
            if (!NamespaceSyntaxTrees.ContainsKey(ns))
            {
                NamespaceSyntaxTrees[ns] = new List<SyntaxTree>();
            }
            NamespaceSyntaxTrees[ns].Add(syntaxTree);
        }
    }

    public void InitializeFromCodeInputs(params string[] codeItems)
    {
        foreach (var code in codeItems)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();
            var ns = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.GetNSName();
            if (ns == null) return;
            if (!NamespaceSyntaxTrees.ContainsKey(ns))
            {
                NamespaceSyntaxTrees[ns] = new List<SyntaxTree>();
            }
            NamespaceSyntaxTrees[ns].Add(syntaxTree);
        }        
    }


    public string[] GetNameSpaces()
    {
        return NamespaceSyntaxTrees.Keys.ToArray();
    }

    public virtual TemplateClassInfo? GetClassInfo(string ns, string className)
    {
        if (!NamespaceSyntaxTrees.ContainsKey(ns)) return null;
        var syntaxTrees = NamespaceSyntaxTrees[ns];
        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var cl in classes)
            {
                if (cl.GetClassName() == className)
                {
                    var props = cl.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                    var classInfo = new TemplateClassInfo
                    {
                        ClassName = cl.GetClassName(),
                        ClassProperties = props.Select(prop => new ClassPropertyInfo
                        {
                            IsArray = prop.IsArrayLike(),
                            IsNullable = prop.Type.ToFullString().Contains("?"),
                            PropName = prop.GetPropName(),
                            PropTypeName = prop.GetSimplifiedPropTypeName()
                        }).ToList()
                    };
                    return classInfo;
                }
            }
        }
        return null;
    }

    public virtual IEnumerable<TemplateClassInfo> GetNSClasses(string ns)
    {
        if (!NamespaceSyntaxTrees.ContainsKey(ns)) return new List<TemplateClassInfo>();
        var syntaxTrees = NamespaceSyntaxTrees[ns];
        var classesInfo = new List<TemplateClassInfo>();
        foreach (var syntaxTree in syntaxTrees)
        {
            var root = syntaxTree.GetRoot();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var cl in classes)
            {
                var clInfo = GetClassInfo(ns, cl.GetClassName());
                classesInfo.Add(clInfo!);
            }
        }
        return classesInfo;
    }
}
