using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;
using TestsCommon;

namespace CsharpV1ExecutionTests
{
    [TestFixture]
    public class SnippetExecutionV1Tests
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
        /// Gets TestCaseData for V1
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetExecutionTestData(
            new RunSettings
            {
                Version = Versions.V1,
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
        [RetryTestCaseSourceAttribute(typeof(SnippetExecutionV1Tests), nameof(TestDataV1), MaxTries = 3)]
        public async Task Test(ExecutionTestData testData)
        {
            await CSharpTestRunner.Execute(testData, publicClientApp, confidentialClientApp);
        }
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
        public class RetryTestCaseSourceAttribute : TestCaseSourceAttribute, IRepeatTest
        {
            #region constructors
            public RetryTestCaseSourceAttribute(string sourceName) : base(sourceName){}
            public RetryTestCaseSourceAttribute(Type sourceType) : base(sourceType){}
            public RetryTestCaseSourceAttribute(Type sourceType, string sourceName) : base(sourceType, sourceName){}
            public RetryTestCaseSourceAttribute(string sourceName, object[] methodParams) : base(sourceName, methodParams){}
            public RetryTestCaseSourceAttribute(Type sourceType, string sourceName, object[] methodParams) : base(sourceType, sourceName, methodParams){}
            #endregion

            #region repeat components
            public int MaxTries { get; set; }
            TestCommand ICommandWrapper.Wrap(TestCommand command) => new RetryAttribute.RetryCommand(command, MaxTries);
            #endregion
        }
    }
}
