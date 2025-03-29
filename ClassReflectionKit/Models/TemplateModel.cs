using System.Text;

namespace ClassReflectionKit.Models;

/// <summary>
/// Class for storing information about other classes that will be rendered in a template
/// </summary>
public class TemplateClassInfo
{
    public string ClassName { get; set; } = String.Empty;
    public IEnumerable<ClassPropertyInfo> ClassProperties { get; set; } = new List<ClassPropertyInfo>();
}

public class ClassPropertyInfo
{
    public bool IsNullable { get; init; }
    public bool IsArray { get; init; }

    // #TODO - Leave it here for future implementation
    public bool IsCustomClass { get; set; } = false;

    public required string PropTypeName { get; init; }

    public required string PropName { get; init; }
}
