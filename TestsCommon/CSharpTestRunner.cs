using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using MsGraphSDKSnippetsCompiler.Templates;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;

namespace TestsCommon
{
    /// <summary>
    /// TestRunner for C# compilation tests
    /// </summary>
    public static class CSharpTestRunner
    {
        /// <summary>
        /// matches csharp snippet from C# snippets markdown output
        /// </summary>
        private const string Pattern = @"```csharp(.*)```";

        /// <summary>
        /// compiled version of the C# markdown regular expression
        /// uses Singleline so that (.*) matches new line characters as well
        /// </summary>
        private static readonly Regex RegExp = new Regex(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Embeds C# snippet from docs repo into a compilable template
        /// </summary>
        /// <param name="snippet">code snippet from docs repo</param>
        /// <returns>
        /// C# snippet embedded into compilable template
        /// </returns>
        private static string ConcatBaseTemplateWithSnippet(string snippet)
        {
            //get the base template
            var microsoftGraphShellTemplate = new MSGraphSDKShellTemplate().TransformText();

            // there are mixture of line endings, namely \r\n and \n, normalize that into \r\n
            string codeToCompile = microsoftGraphShellTemplate
                       .Replace("//insert-code-here", snippet)
                       .Replace("\r\n", "\n").Replace("\n", "\r\n");

            return codeToCompile;
        }

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="fileName">C# snippet file name</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        public static void Run(string fileName, Versions version)
        {
            var fullPath = Path.Join(GraphDocsDirectory.GetCsharpSnippetsDirectory(version), fileName);
            var fileContent = File.ReadAllText(fullPath);

            var match = RegExp.Match(fileContent);

            // match groups return the full text in Groups[0] and matched portion in Groups[1]
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

            Assert.Fail(new CompilationOutputMessage(compilationResultsModel, codeToCompile).ToString());
        }
    }
}
