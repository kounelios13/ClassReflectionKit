using ClassReflectionKit.Helpers;
using ClassReflectionKit.Models;
namespace ClassreflectionUtilLib.Models;


public class CustomHelper : ClassReflectionKitHelper
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

    override
        public List<TemplateClassInfo> GetNSClasses(string ns)
    {
        List<TemplateClassInfo> classesInfo = base.GetNSClasses(ns).ToList();
        List<string> classNames = classesInfo.Select(classesInfo => classesInfo.ClassName).ToList();

        foreach (var cl in classesInfo)
        {

            var customTypeProps = cl.ClassProperties.Where(e => classNames.Contains(e.PropTypeName) && e.PropTypeName != cl.ClassName).ToList();
            if (customTypeProps.Count() < 1) continue;
            customTypeProps.ForEach(x =>
            {
                x.IsCustomClass = true;
            });
        }

        return classesInfo;
    }
}
