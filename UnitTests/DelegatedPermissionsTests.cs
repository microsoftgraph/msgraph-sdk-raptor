using MsGraphSDKSnippetsCompiler;

using NUnit.Framework;

namespace UnitTests
{
    public class DelegatedPermissionsTests
    {
        /// <summary>
        ///     Schedule Requires Delegated Permission otherwise
        ///     MS-APP-ACT-AS Header is required
        /// </summary>
        private const string Schedule = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var schedule = graphClient.Teams[\""8730b5a6-eca3-400d-9011-1f4202418218\""].Schedule
                          .Request()
                          .GetHttpRequestMessage();
";
        /// <summary>
        ///     Children of `Me` should be treated as Delegated
        /// </summary>
        private const string People = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var schedule = graphClient.Me.People
                          .Request()
                          .GetHttpRequestMessage();
";
        /// <summary>
        ///     Children of `Me` should be treated as Delegated
        /// </summary>
        private const string Me = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var schedule = graphClient.Me
                          .Request()
                          .GetHttpRequestMessage();
";
        /// <summary>
        ///     Group should not be treated as Delegated
        /// </summary>
        private const string Groups = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var schedule = graphClient.Groups[\""8730b5a6-eca3-400d-9011-1f4202418218\""].Drive
                          .Request()
                          .GetHttpRequestMessage();
";

        /// <summary>
        ///     All Snippets in Test Cases should be marked as requiring Delegated permission
        /// </summary>
        /// <param name="testSnippet"></param>
        [TestCase(Me)]
        [TestCase(People)]
        [TestCase(Schedule)]
        public void ShouldBeMarkedAsDelegated(string testSnippet)
        {
            var requiredDelegatedPermissions = MicrosoftGraphCSharpCompiler.RequiresDelegatedPermissions(testSnippet);
            Assert.True(requiredDelegatedPermissions);
        }

        /// <summary>
        ///     Groups Snippet Should not be marked as Delegated
        /// </summary>
        /// <param name="testSnippet"></param>
        [TestCase(Groups)]
        public void ShouldNotBeMarkedDelegated(string testSnippet)
        {
            var requiredDelegatedPermissions = MicrosoftGraphCSharpCompiler.RequiresDelegatedPermissions(testSnippet);
            Assert.False(requiredDelegatedPermissions);
        }
    }
}
