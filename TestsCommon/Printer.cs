using MsGraphSDKSnippetsCompiler.Models;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TestsCommon
{
    public static class Printer
    {
        public static string CompilerErrors(CompilationResultsModel model)
        {
            if (model.Diagnostics == null)
            {
                return "No diagnostics from the compiler!";
            }

            var result = new StringBuilder("\r\n");
            foreach (var diagnostic in model.Diagnostics)
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                var line = lineSpan.StartLinePosition.Line + 1; // 0 indexed
                var lineStart = lineSpan.StartLinePosition.Character;

                result.Append($"\r\n{diagnostic.Id}: ({line},{lineStart}) {diagnostic.GetMessage(CultureInfo.InvariantCulture)}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Adds line numbers to a piece of code for easy reference
        /// </summary>
        /// <param name="code">code to be printed</param>
        /// <returns></returns>
        public static string AddLineNumbers(string code)
        {
            var lines = code.Split("\r\n");

            var widestLineNumberStringLength = lines.Length.ToString().Length;

            var builder = new StringBuilder("\r\n");
            for (int lineNumber = 1; lineNumber < lines.Length + 1; lineNumber++)
            {
                builder.Append(lineNumber.ToString().PadLeft(widestLineNumberStringLength)) // align line numbers to the right
                       .Append(" ")
                       .Append(lines[lineNumber - 1])
                       .Append("\r\n");
            }

            return builder.ToString();
        }
    }
}
