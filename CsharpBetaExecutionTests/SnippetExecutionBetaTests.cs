using System.Collections.Generic;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace CsharpBetaExecutionTests
{
    [TestFixture]
    public class SnippetExecutionBetaTests
    {
        /// <summary>
        /// Gets TestCaseData for Beta
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetExecutionTestData(
            new RunSettings
            {
                Version = Versions.Beta,
                Language = Languages.CSharp,
                KnownFailuresRequested = false
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(SnippetExecutionBetaTests), nameof(TestDataBeta))]
        public void Test(ExecutionTestData testData)
        {
            CSharpTestRunner.Execute(testData);
        }
    }
}
