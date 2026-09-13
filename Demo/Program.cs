// See https://aka.ms/new-console-template for more information
using ClassReflectionKit.Extensions;
using ClassReflectionKit.Helpers;
using ClassreflectionUtilLib.Models;

// #region Helper Init
var helper = new ClassReflectionKitHelper();
var customHelper = new CustomHelper();
// Resolve files relative to the deployed application, where Samples is copied by the project file.
var samplesDirectory = Path.Combine(AppContext.BaseDirectory, "Samples");
var samples = new List<string> { "Address.cs", "User.cs" , "Employee.cs" }
    .Select(x => Path.Combine(samplesDirectory, x)).ToList();

var codeSamples = samples.Select(File.ReadAllText).ToArray();
helper.InitializeFromCodeInputs(codeSamples);
customHelper.InitializeFromCodeInputs(codeSamples);
// #endregion

// #region Delegates

static ClassReflectionKit.Models.ProcessClassInfo HandleNullableProps()
{
    return (c) =>
    {
        var props = c.ClassProperties.Select(s => s.PropName).ToArray();
        var formatted = string.Join(',', props);
        var nullableProps = props.Where(s => s.EndsWith("Specified")).ToArray();
        foreach (var nprop in nullableProps)
        {
            var correspondingPropName = nprop.Replace("Specified", "");
            Console.WriteLine($"Will edit prop {correspondingPropName}");
            var prop = c.ClassProperties.Find(e => e.PropName == correspondingPropName);
            if (prop == null) continue;
            prop.IsNullable = true;
        }
        c.ClassProperties = c.ClassProperties.Where(
            prop => !nullableProps.Contains(prop.PropName)
        ).ToList();
        return c;
    };
}


static ClassReflectionKit.Models.ProcessNSClasses HandleCustomPropsClasses()
{
    return e =>
    {
        var availableClassNames = e.Select(s => s.ClassName).ToArray();
        foreach(var c in e)
        {
            // Iterate through each prop. If the propTypeName is included in the availableClassNames mark it as CustomType
            foreach(var prop in c.ClassProperties){
                if(availableClassNames.Contains(prop.PropTypeName)){
                    prop.IsCustomClass = true;
                }
            }
        }
        return e;
    };
}
// #endregion


foreach (var ns in helper.GetNameSpaces())
{
    Console.WriteLine("Namespace: " + ns);
    var classes = helper.GetNSClasses(ns, HandleCustomPropsClasses(), HandleNullableProps())
        .ToArray();
    foreach (var cl in classes)
    {
        Console.WriteLine(cl.ToTemplateString());
    }
}
