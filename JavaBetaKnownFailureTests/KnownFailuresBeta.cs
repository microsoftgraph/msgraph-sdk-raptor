using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace JavaBetaKnownFailureTests
{
    [TestFixture]
    public class KnownFailuresBeta
    {
        /// <summary>
        /// Gets TestCaseData for Beta known failures
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetTestCaseData(
            new RunSettings
            {
                Version = Versions.Beta,
                Language = Languages.Java,
                KnownFailuresRequested = true
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(KnownFailuresBeta), nameof(TestDataBeta))]
        public void Test(LanguageTestData testData)
        {
            JavaTestRunner.Run(testData);
        }
    }
}