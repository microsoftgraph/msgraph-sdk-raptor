// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using System.Globalization;
using System.Text;

namespace TestsCommon
{
    /// <summary>
    /// String represenation of compilation errors
    /// </summary>
    public class CompilationOutputMessage
    {
        /// <summary>
        /// Compilation result
        /// </summary>
        private readonly CompilationResultsModel Model;

        /// <summary>
        /// Code that was attempted to be compiled
        /// </summary>
        private readonly string Code;

        /// <summary>
        /// Docs page where the snippet is shown
        /// </summary>
        private readonly string DocsLink;

        /// <summary>
        /// Message for known issue
        /// </summary>
        private readonly string KnownIssueMessage;

        /// <summary>
        /// Whether the issue is known or not
        /// </summary>
        private readonly bool IsKnownIssue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Compilation result</param>
        /// <param name="code">Code that was attempted to be compiled</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="knownIssueMessage">message for known issue</param>
        public CompilationOutputMessage(
            CompilationResultsModel model,
            string code,
            string docsLink,
            string knownIssueMessage,
            bool isKnownIssue)
        {
            Model = model;
            Code = code;
            DocsLink = docsLink;
            KnownIssueMessage = knownIssueMessage;
            IsKnownIssue = isKnownIssue;
        }

        /// <summary>
        /// String representation of compilation result
        /// </summary>
        /// <returns>
        /// Code with line numbers and compiler errors
        /// </returns>
        public override string ToString() => GetKnownIssueMessage() + GetDocsLink() + GetCodeWithLineNumbers() + CompilerErrors();

        /// <summary>
        /// Get string representation of known issue message
        /// </summary>
        /// <returns>Known issue message if the issue is known, empty string if not</returns>
        private string GetKnownIssueMessage() => IsKnownIssue ? $"Known Issue: {KnownIssueMessage}\r\n" : string.Empty;

        /// <summary>
        /// Gets documentation link for the snippet
        /// </summary>
        /// <returns>Documentation link for the snippet</returns>
        private string GetDocsLink() => $"[Docs Link]({DocsLink})\r\n";

        /// <summary>
        /// For each compiler error, generates an error string with code location references
        /// </summary>
        /// <returns>
        /// diagnostic-id: (line, character) error-message
        /// e.g. C1000: (25,8) Cannot convert type from string to byte[]
        /// </returns>
        private string CompilerErrors()
        {
            if (Model?.Diagnostics == null)
            {
                return "No diagnostics from the compiler!";
            }

            var result = new StringBuilder("\r\n");
            foreach (var diagnostic in Model.Diagnostics)
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                var line = lineSpan.StartLinePosition.Line + 1; // 0 indexed
                var column = lineSpan.StartLinePosition.Character;

                result.Append($"\r\n{diagnostic.Id}: (Line:{line}, Column:{column}) {diagnostic.GetMessage(CultureInfo.InvariantCulture)}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Adds line numbers to a piece of code for easy reference
        /// Line numbers are right-aligned
        /// </summary>
        /// <returns>
        /// Code decorated with line numbers
        /// </returns>
        private string GetCodeWithLineNumbers()
        {
            if (string.IsNullOrEmpty(Code))
            {
                return string.Empty;
            }

            var lines = Code.Split("\r\n");

            var widestLineNumberWidth = lines.Length.ToString(CultureInfo.InvariantCulture).Length;

            var builder = new StringBuilder("\r\n```\r\n");
            for (int lineNumber = 1; lineNumber < lines.Length + 1; lineNumber++)
            {
                builder.Append(lineNumber.ToString(CultureInfo.InvariantCulture).PadLeft(widestLineNumberWidth)) // align line numbers to the right
                       .Append(' ')
                       .Append(lines[lineNumber - 1])
                       .Append("\r\n");
            }

            builder.Append("```\r\n");

            return builder.ToString();
        }
    }
}
