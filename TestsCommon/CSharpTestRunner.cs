// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
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
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"using System;
using Microsoft.Graph;
using MsGraphSDKSnippetsCompiler;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// Disambiguate colliding namespaces
using DayOfWeek = Microsoft.Graph.DayOfWeek;
using TimeOfDay = Microsoft.Graph.TimeOfDay;
using KeyValuePair = Microsoft.Graph.KeyValuePair;

public class GraphSDKTest
{
    private IAuthenticationProvider authProvider = null;

    private async void Main()
    {
        authProvider = AuthenticationProvider.GetIAuthenticationProvider();

        #region msgraphsnippets
        //insert-code-here
        #endregion
    }
}";

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
            // there are mixture of line endings, namely \r\n and \n, normalize that into \r\n
            string codeToCompile = SDKShellTemplate
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
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Run(CsharpTestData testData)
        {
            if (testData == null)
            {
                throw new ArgumentNullException(nameof(testData));
            }

            var fullPath = Path.Join(GraphDocsDirectory.GetCsharpSnippetsDirectory(testData.Version), testData.FileName);
            Assert.IsTrue(File.Exists(fullPath), "Snippet file referenced in documentation is not found!");

            var fileContent = File.ReadAllText(fullPath);
            var match = RegExp.Match(fileContent);
            Assert.IsTrue(match.Success, "Csharp snippet file is not in expected format!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ")                      // do not use tabs
                .Replace("\r\n\r\n\r\n", "\r\n\r\n");       // do not have two consecutive empty lines

            var codeToCompile = ConcatBaseTemplateWithSnippet(codeSnippetFormatted);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, testData.Version);

            if (compilationResultsModel.IsSuccess)
            {
                Assert.Pass();
            }

            var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);

            // environment variable for sources directory is defined only for cloud runs
            var config = AppSettings.Config();
            if (bool.Parse(config.GetSection("IsLocalRun").Value)
                && bool.Parse(config.GetSection("GenerateLinqPadOutputInLocalRun").Value))
            {
                WriteLinqFile(testData, codeSnippetFormatted);
            }

            Assert.Fail($"{compilationOutputMessage}");
        }

        /// <summary>
        /// Generates .linq file in default My Queries folder so that the results are visible in LinqPad right away
        /// </summary>
        /// <param name="testData">test data</param>
        /// <param name="codeSnippetFormatted">code snippet</param>
        private static void WriteLinqFile(CsharpTestData testData, string codeSnippetFormatted)
        {
            var linqPadQueriesDefaultFolder = Path.Join(
                    Environment.GetEnvironmentVariable("USERPROFILE"),
                    "/Documents",
                    "/LINQPad Queries");

            var linqDirectory = Path.Join(
                    linqPadQueriesDefaultFolder,
                    "/RaptorResults",
                    (testData.Version, testData.IsKnownIssue) switch
                    {
                        (Versions.Beta, false) => "/Beta",
                        (Versions.Beta, true) => "/BetaKnown",
                        (Versions.V1, false) => "/V1",
                        (Versions.V1, true) => "/V1Known",
                        _ => throw new ArgumentException("unsupported version", nameof(testData))
                    });

            Directory.CreateDirectory(linqDirectory);

            var linqFilePath = Path.Join(linqDirectory, testData.FileName.Replace(".md", ".linq"));

            const string LinqTemplateStart = "<Query Kind=\"Statements\">";
            const string LinqTemplateEnd =
@"
  <Namespace>DayOfWeek = Microsoft.Graph.DayOfWeek</Namespace>
  <Namespace>KeyValuePair = Microsoft.Graph.KeyValuePair</Namespace>
  <Namespace>Microsoft.Graph</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TimeOfDay = Microsoft.Graph.TimeOfDay</Namespace>
</Query>

IAuthenticationProvider authProvider  = null;
";

            File.WriteAllText(linqFilePath,
                LinqTemplateStart
                + Environment.NewLine
                + (testData.Version) switch
                    {
                        Versions.Beta => "  <NuGetReference Prerelease=\"true\">Microsoft.Graph.Beta</NuGetReference>",
                        Versions.V1 => "  <NuGetReference>Microsoft.Graph</NuGetReference>",
                        _ => throw new ArgumentException("unsupported version", nameof(testData))
                    }
                + LinqTemplateEnd
                + codeSnippetFormatted.Replace("\n        ", "\n"));
        }
    }
}
