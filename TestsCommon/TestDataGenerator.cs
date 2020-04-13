using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestsCommon
{
    public static class TestDataGenerator
    {
        public static IEnumerable<TestCaseData> GetTestCaseData(Versions version)
        {
            var directory = GraphDocsDirectory.GetDirectory(version);
            return from file in Directory.GetFiles(directory, "*.md")
                   let fileName = Path.GetFileName(file)                            // e.g. application-addpassword-csharp-snippets.md
                   let testNamePostfix = version.ToString() + "-compiles"           // e.g. Beta-compiles
                   let testName = fileName.Replace("snippets.md", testNamePostfix)  // e.g. application-addpassword-csharp-Beta-compiles
                   select new TestCaseData(fileName, version).SetName(testName);
        }
    }
}
