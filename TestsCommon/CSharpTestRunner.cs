﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

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
using System.Net.Http;

// Disambiguate colliding namespaces
using DayOfWeek = Microsoft.Graph.DayOfWeek;
using TimeOfDay = Microsoft.Graph.TimeOfDay;
using KeyValuePair = Microsoft.Graph.KeyValuePair;

public class GraphSDKTest
{
    public async Task Main(IAuthenticationProvider authProvider)
    {
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
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Compile(LanguageTestData testData)
        {
            if (testData == null)
            {
                throw new ArgumentNullException(nameof(testData));
            }

            var fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, Languages.CSharp), testData.FileName);
            Assert.IsTrue(File.Exists(fullPath), "Snippet file referenced in documentation is not found!");

            var fileContent = File.ReadAllText(fullPath);
            var (codeToCompile, codeSnippetFormatted) = GetCodeToCompile(fileContent);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, testData.Version);

            var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);
            EvaluateCompilationResult(compilationResultsModel, testData, codeSnippetFormatted, compilationOutputMessage);

            Assert.Pass();
        }

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="executionTestData">Test data containing information such as snippet file name</param>
        public static void Execute(ExecutionTestData executionTestData)
        {
            if (executionTestData == null)
            {
                throw new ArgumentNullException(nameof(executionTestData));
            }

            var testData = executionTestData.LanguageTestData;

            var (codeToCompile, codeSnippetFormatted) = GetCodeToCompile(executionTestData.FileContent, CaptureUriAndHeadersInException);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var executionResultsModel = microsoftGraphCSharpCompiler.ExecuteSnippet(codeToCompile, testData.Version);
            var compilationOutputMessage = new CompilationOutputMessage(
                executionResultsModel.CompilationResult,
                codeToCompile,
                testData.DocsLink,
                testData.KnownIssueMessage,
                testData.IsKnownIssue);

            EvaluateCompilationResult(executionResultsModel.CompilationResult, testData, codeSnippetFormatted, compilationOutputMessage);

            if (!executionResultsModel.Success)
            {
                Assert.Fail($"{compilationOutputMessage}{Environment.NewLine}{executionResultsModel.ExceptionMessage}");
            }

            Assert.Pass(compilationOutputMessage.ToString());
        }

        private static void EvaluateCompilationResult(CompilationResultsModel compilationResult, LanguageTestData testData, string codeSnippetFormatted, CompilationOutputMessage compilationOutputMessage)
        {
            if (!compilationResult.IsSuccess)
            {
                // environment variable for sources directory is defined only for cloud runs
                var config = AppSettings.Config();
                if (bool.Parse(config.GetSection("IsLocalRun").Value)
                    && bool.Parse(config.GetSection("GenerateLinqPadOutputInLocalRun").Value))
                {
                    WriteLinqFile(testData, codeSnippetFormatted);
                }

                Assert.Fail($"{compilationOutputMessage}");
            }
        }

        private static string CaptureUriAndHeadersInException(string codeToCompile)
        {
            var resultVariablePattern = "var ([a-zA-Z0-9]+) = await graphClient";
            string resultVariable = null;
            try
            {
                resultVariable = Regex.Match(codeToCompile, resultVariablePattern).Groups[1].Value;
            }
            catch (Exception e)
            {
                Assert.Fail("result variable is not found!" + Environment.NewLine + e.Message);
            }

            codeToCompile = codeToCompile.Replace("await graphClient", "graphClient")
                .Replace(".GetAsync();", $@".GetHttpRequestMessage();

        var uri = {resultVariable}.RequestUri;
        var headers = {resultVariable}.Headers;
        {resultVariable}.Method = HttpMethod.Get;
        try
        {{
            await graphClient.HttpProvider.SendAsync({resultVariable});
        }}
        catch (Exception e)
        {{
            throw new Exception($""Request URI: {{uri}}{{Environment.NewLine}}Request Headers:{{Environment.NewLine}}{{headers}}"", e);
        }}");

            return codeToCompile;
        }

        private static (string, string) GetCodeToCompile(string fileContent, Func<string, string> postTransform = null)
        {
            var match = RegExp.Match(fileContent);
            Assert.IsTrue(match.Success, "Csharp snippet file is not in expected format!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ")                      // do not use tabs
                .Replace("\r\n\r\n\r\n", "\r\n\r\n");       // do not have two consecutive empty lines

            var codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(codeSnippetFormatted, SDKShellTemplate);

            if (postTransform == null)
            {
                return (codeToCompile, codeSnippetFormatted);
            }

            return (postTransform(codeToCompile), codeSnippetFormatted);
        }

        /// <summary>
        /// Generates .linq file in default My Queries folder so that the results are visible in LinqPad right away
        /// </summary>
        /// <param name="testData">test data</param>
        /// <param name="codeSnippetFormatted">code snippet</param>
        private static void WriteLinqFile(LanguageTestData testData, string codeSnippetFormatted)
        {
            var linqPadQueriesDefaultFolder = Path.Join(
                    Environment.GetEnvironmentVariable("USERPROFILE"),
                    "/OneDrive - Microsoft", // remove this if you are not syncing your Documents to OneDrive
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
            string LinqTemplateEnd =
@$"
  <Namespace>DayOfWeek = Microsoft.Graph.DayOfWeek</Namespace>
  <Namespace>KeyValuePair = Microsoft.Graph.KeyValuePair</Namespace>
  <Namespace>Microsoft.Graph</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TimeOfDay = Microsoft.Graph.TimeOfDay</Namespace>
</Query>
// {testData.DocsLink}{(testData.IsKnownIssue ? (Environment.NewLine + "/* " + testData.KnownIssueMessage + " */") : string.Empty)}
IAuthenticationProvider authProvider  = null;";

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
