namespace ClassReflectionKit.Models;

/// <summary>
/// Class for storing information about other classes that will be rendered in a template
/// </summary>
public class TemplateClassInfo
{
    public string ClassName { get; set; } = String.Empty;
    public string NameSpace {  get; set; } = String.Empty;
    public List<ClassPropertyInfo> ClassProperties { get; set; } = new();
}

public class ClassPropertyInfo
{
    public bool IsNullable { get; set; }
    public bool IsArray { get; init; }

    public bool IsCustomClass { get; set; } = false;

    public required string PropTypeName { get; init; }

    public required string PropName { get; set; }
}
