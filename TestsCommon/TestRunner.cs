using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using MsGraphSDKSnippetsCompiler.Templates;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;

namespace TestsCommon
{
    public static class TestRunner
    {
        private const string Pattern = @"```csharp(.*)```";
        private static readonly Regex Regex = new Regex(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);

        private static string ConcatBaseTemplateWithSnippet(string fileContent)
        {
            //get the base template
            var microsoftGraphShellTemplate = new MSGraphSDKShellTemplate().TransformText();

            // there are mixture of line endings, namely \r\n and \n, normalize that into \r\n
            string codeToCompile = microsoftGraphShellTemplate
                       .Replace("//insert-code-here", fileContent)
                       .Replace("\r\n", "\n").Replace("\n", "\r\n");

            return codeToCompile;
        }

        public static void Run(string fileName, Versions version)
        {
            var fullPath = Path.Join(GraphDocsDirectory.GetDirectory(version), fileName);
            var fileContent = File.ReadAllText(fullPath);

            var match = Regex.Match(fileContent);
            Assert.AreEqual(2, match.Groups.Count, "There should be only one match!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ")                      // do not use tabs
                .Replace("\r\n\r\n\r\n", "\r\n\r\n");       // do not have two consecutive empty lines

            var codeToCompile = ConcatBaseTemplateWithSnippet(codeSnippetFormatted);

            //Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(fileName);
            var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, version);

            if (compilationResultsModel.IsSuccess)
            {
                Assert.Pass();
            }

            Assert.Fail(Printer.AddLineNumbers(codeToCompile) + Printer.CompilerErrors(compilationResultsModel));
        }

    }
}
