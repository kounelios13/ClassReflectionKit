using ClassReflectionKit.Models;

namespace Demo.Models;

public class CustomHelper : ClassReflectionKit.Helpers.ClassReflectionKitHelper
{
    override
    public TemplateClassInfo? GetClassInfo(string ns, string className)
    {
        var classInfo = base.GetClassInfo(ns, className);
        if (classInfo == null) return null;
        classInfo.ClassProperties.ToList()
            .ForEach(x => x.IsNullable = false);
        var nullableProps = classInfo.ClassProperties.Where(x => x.PropName.EndsWith("Specified"))
            .Select(x => x.PropName).ToList();
        if (nullableProps.Count() < 1) return classInfo;
        foreach (var prop in nullableProps)
        {
            // Find corresponding property that  does not end with "Specified"
            var propName = prop.Replace("Specified", "");
            var correspondingProp = classInfo.ClassProperties.FirstOrDefault(x => x.PropName == propName);
            if (correspondingProp == null) continue;
            correspondingProp.IsNullable = true;
        }
        // Keep only properties that do not end with "Specified"
        classInfo.ClassProperties = classInfo.ClassProperties.
            Where(x => !nullableProps.Contains(x.PropName))
            .ToList();
        return classInfo;
    }
}
