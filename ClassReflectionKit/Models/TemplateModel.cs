
namespace ClassReflectionKit.Models;

/// <summary>
/// Class for storing information about other classes that will be rendered in a template
/// </summary>
///
public class TemplateClassInfo
{
    public string ClassName { get; set; } = String.Empty;
    public string NameSpace { get; set; } = String.Empty;
    public List<ClassPropertyInfo> ClassProperties { get; set; } = new();
    public List<MetaDataInfo> MetaData = new();

}

public class ClassPropertyInfo
{
    public bool IsNullable { get; set; }
    public bool IsArray { get; set; }

    public bool IsCustomClass { get; set; } = false;


    public string PropTypeName { get; set; } = String.Empty;

    public string PropName { get; set; } = String.Empty;
    /// <summary>
    /// Allow defining custom metadata for extra manipulation
    /// </summary>
    public List<MetaDataInfo> MetaData = new();
}

/// <summary>
/// Hold Metadata for different scenarios
/// </summary>
public class MetaDataInfo
{
    public string Key = string.Empty;
    public string Value = string.Empty;
}

public delegate TemplateClassInfo? ProcessClassInfo(TemplateClassInfo? classInfo);

public delegate List<TemplateClassInfo> ProcessNSClasses(List<TemplateClassInfo> classes);