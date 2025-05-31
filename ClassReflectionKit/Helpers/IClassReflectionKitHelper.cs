using ClassReflectionKit.Models;

namespace ClassReflectionKit.Helpers;

public interface IClassReflectionKitHelper
{
    TemplateClassInfo? GetClassInfo(string ns, string className);
    TemplateClassInfo? GetClassInfo(string ns, string className, ProcessClassInfo procDelegate);
    string[] GetNameSpaces();
    List<TemplateClassInfo> GetNSClasses(string ns);
    List<TemplateClassInfo> GetNSClasses(string ns, ProcessNSClasses procDelegate);
    List<TemplateClassInfo> GetNSClasses(string ns, ProcessNSClasses procDelegate, ProcessClassInfo procClassDelegate);
    void InitializeFromCodeInputs(params string[] codeItems);
    void InitializeFromFilePaths(params string[] filePaths);

    bool IsCodeSnippetValid(string codeSnippet);
}