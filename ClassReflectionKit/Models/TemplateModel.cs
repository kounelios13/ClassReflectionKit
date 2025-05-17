using Microsoft.JavaScript.NodeApi;

namespace ClassReflectionKit.Models;

/// <summary>
/// Class for storing information about other classes that will be rendered in a template
/// </summary>
///
[JSExport("TemplateClassInfo")]
public class TemplateClassInfo
{
    public string ClassName { get; set; } = String.Empty;
    public string NameSpace {  get; set; } = String.Empty;
    public List<ClassPropertyInfo> ClassProperties { get; set; } = new();
}

[JSExport("ClassPropertyInfo")]
public class ClassPropertyInfo
{
    public bool IsNullable { get; set; }
    public bool IsArray { get; set; }

    public bool IsCustomClass { get; set; } = false;

    public string PropTypeName { get; set; } = String.Empty;

    public string PropName { get; set; } = String.Empty;
}

[JSExport("ProcessClassInfo")]
public delegate TemplateClassInfo? ProcessClassInfo(TemplateClassInfo? classInfo);

[JSExport("ProcessNSClasses")]
public delegate IEnumerable<TemplateClassInfo> ProcessNSClasses(IEnumerable<TemplateClassInfo> classes);