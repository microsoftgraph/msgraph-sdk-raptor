using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using TestsCommon;

namespace CsharpBetaExecutionTests
{
    [TestFixture]
    public class SnippetExecutionBetaTests
    {
        public static IConfidentialClientApplication confidentialClientApp;
        public static IPublicClientApplication publicClientApp;

        [OneTimeSetUp]
        public void Init()
        {
            var config = AppSettings.Config();
            var clientId = config.GetNonEmptyValue("ClientID");
            var authority = config.GetNonEmptyValue("Authority");
            var username = config.GetNonEmptyValue("Username");
            var password = config.GetNonEmptyValue("Password");
            // application permissions
            var tenantId = config.GetNonEmptyValue("TenantID");
            var clientSecret = config.GetNonEmptyValue("ClientSecret");
            publicClientApp = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).Build();
            confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

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
            await CSharpTestRunner.Execute(testData,publicClientApp,confidentialClientApp);
        }
    }
}
