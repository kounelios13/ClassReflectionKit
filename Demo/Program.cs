// See https://aka.ms/new-console-template for more information
using ClassReflectionKit.Extensions;
using Demo.Models;

var helper = new CustomHelper();


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
