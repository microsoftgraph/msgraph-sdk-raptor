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
        /// Constructor
        /// </summary>
        /// <param name="model">Compilation result</param>
        /// <param name="code">Code that was attempted to be compiled</param>
        public CompilationOutputMessage(CompilationResultsModel model, string code)
        {
            Model = model;
            Code = code;
        }

        /// <summary>
        /// String representation of compilation result
        /// </summary>
        /// <returns>
        /// Code with line numbers and compiler errors
        /// </returns>
        public override string ToString() => GetCodeWithLineNumbers() + CompilerErrors();

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

            var builder = new StringBuilder("\r\n");
            for (int lineNumber = 1; lineNumber < lines.Length + 1; lineNumber++)
            {
                builder.Append(lineNumber.ToString(CultureInfo.InvariantCulture).PadLeft(widestLineNumberWidth)) // align line numbers to the right
                       .Append(" ")
                       .Append(lines[lineNumber - 1])
                       .Append("\r\n");
            }

            return builder.ToString();
        }
    }
}
