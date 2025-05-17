// See https://aka.ms/new-console-template for more information
using ClassReflectionKit.Extensions;
using ClassReflectionKit.Helpers;
using ClassReflectionKit.Models;
using ClassreflectionUtilLib.Models;



var classInfoDelegate = new ProcessClassInfo(e => { 
    if (e == null)
    {
        return null;
    }
    e.ClassProperties.ToList()
        .ForEach(x => x.IsNullable = false);
    var nullableProps = e.ClassProperties.Where(x => x.PropName.EndsWith("Specified"));
    if (nullableProps.Count() < 1) return e;
    foreach (var prop in nullableProps)
    {
        // Find corresponding property that  does not end with "Specified"
        var propName = prop.PropName.Replace("Specified", "");
        var correspondingProp = e.ClassProperties.FirstOrDefault(x => x.PropName == propName);
        if (correspondingProp == null) continue;
        correspondingProp.IsNullable = true;
    }
    // Keep only properties that do not end with "Specified"
    e.ClassProperties = e.ClassProperties.
        Where(x => !nullableProps.Select(x => x.PropName).Contains(x.PropName))
        .ToList();
    return e; 
});

var processNSClassesDelegate = new ProcessNSClasses(e => { return e; });

var helper = new ClassReflectionKitHelper();


var samples = new List<string> { "Address.cs", "User.cs" , "Employee.cs" };
//Map each sample file to combine it with the current directory
samples = samples.Select(x => Path.Combine(Directory.GetCurrentDirectory(), "Samples", x)).ToList();
helper.InitializeFromFilePaths(samples.ToArray());
foreach (var ns in helper.GetNameSpaces())
{
    Console.WriteLine("Namespace: " + ns);

    var classes = helper.GetNSClasses(ns);
    foreach (var cl in classes)
    {
        Console.WriteLine(cl.ToTemplateString());
    }
}


var fromCodeHelper = new CustomHelper();
var codeSamples = samples.Select(x => File.ReadAllText(x)).ToArray();
fromCodeHelper.InitializeFromCodeInputs(codeSamples);
foreach (var ns in fromCodeHelper.GetNameSpaces())
{
    Console.WriteLine("Namespace: " + ns);
    var classes = fromCodeHelper.GetNSClasses(ns);
    foreach (var cl in classes)
    {
        Console.WriteLine(cl.ToTemplateString());
    }
}
