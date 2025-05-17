using ClassReflectionKit.Models;

namespace ClassReflectionKit.Helpers;

public interface IClassReflectionKitHelper
{
    TemplateClassInfo? GetClassInfo(string ns, string className);
    TemplateClassInfo? GetClassInfo(string ns, string className, ProcessClassInfo procDelegate);
    string[] GetNameSpaces();
    IEnumerable<TemplateClassInfo> GetNSClasses(string ns);
    IEnumerable<TemplateClassInfo> GetNSClasses(string ns, ProcessNSClasses procDelegate);
    IEnumerable<TemplateClassInfo> GetNSClasses(string ns, ProcessNSClasses procDelegate, ProcessClassInfo procClassDelegate);
    void InitializeFromCodeInputs(params string[] codeItems);
    void InitializeFromFilePaths(params string[] filePaths);

    bool IsCodeSnippetValid(string codeSnippet);
}