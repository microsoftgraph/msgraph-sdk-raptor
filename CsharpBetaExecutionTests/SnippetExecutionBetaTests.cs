using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;

using NUnit.Framework;

using TestsCommon;

namespace CsharpBetaExecutionTests
{
    [TestFixture]
    public class SnippetExecutionBetaTests
    {
        private IConfidentialClientApplication _confidentialClientApp;
        private IPublicClientApplication _publicClientApp;
        private RaptorConfig _raptorConfig;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _raptorConfig = TestsSetup.GetConfig();
            _publicClientApp = TestsSetup.SetupPublicClientApp(_raptorConfig);
            _confidentialClientApp = TestsSetup.SetupConfidentialClientApp(_raptorConfig);
        }

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
        [RetryTestCaseSource(typeof(SnippetExecutionBetaTests), nameof(TestDataBeta), MaxTries = 3)]
        public async Task Test(ExecutionTestData testData)
        {
            await CSharpTestRunner.Execute(testData, _raptorConfig, _publicClientApp, _confidentialClientApp);
        }
    }
}
