using NUnit.Framework;

using TestsCommon;

namespace UnitTests
{
    /// <summary>
    ///     Checks that the CSharpTestRunner Regex is able to
    ///     get identifiers for all identifiers as would be valid C#
    ///     https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
    /// </summary>
    public class CSharpTestRunnerTests
    {
        /// <summary>
        ///     Verbatim Identifier such as @event
        ///     https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/verbatim
        /// </summary>
        private const string VerbatimIdentifer = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var @event = await graphClient.Groups[\""8730b5a6-eca3-400d-9011-1f4202418218\""]
                                .Events[\""<group_event>\""]
                                .Request()
                                .GetAsync();
";
        private const string UnderScoreIdentifier = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var _event = await graphClient.Groups[\""8730b5a6-eca3-400d-9011-1f4202418218\""]
                                .Events[\""<group_event>\""]
                                .Request()
                                .GetAsync();
";
        /// <summary>
        ///     @_event is an allowed identifier
        /// </summary>
        private const string VerbatimIdentifierWithUnderScore = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var @_event = await graphClient.Groups[\""8730b5a6-eca3-400d-9011-1f4202418218\""]
                                .Events[\""<group_event>\""]
                                .Request()
                                .GetAsync();
";
        private const string IncorrectVerbatimIdentifier = @"
GraphServiceClient graphClient = new GraphServiceClient( authProvider );
var ev@ent = await graphClient.Groups[\""8730b5a6-eca3-400d-9011-1f4202418218\""]
                                .Events[\""<group_event>\""]
                                .Request()
                                .GetAsync();
";

        [TestCase("@event", VerbatimIdentifer, TestName = "@event")]
        [TestCase("@_event", VerbatimIdentifierWithUnderScore, TestName = "@_event")]
        [TestCase("_event", UnderScoreIdentifier, TestName = "_event")]
        public void ShouldHandleVerbatimVariableIdentifier(string variableName, string testSnippet)
        {
            var match = CSharpTestRunner.ResultVariableRegex.Match(testSnippet);
            Assert.True(match.Success);
            Assert.AreEqual(match.Groups.Count, 2);
            Assert.AreEqual(match.Groups[1].Value, variableName);
        }

        [TestCase("@event", VerbatimIdentifer, TestName = "@event")]
        [TestCase("@_event", VerbatimIdentifierWithUnderScore, TestName = "@_event")]
        [TestCase("_event", UnderScoreIdentifier, TestName = "_event")]
        public void ShouldReturnHttpRequestMessage(string variableName, string testSnippet)
        {
            Assert.DoesNotThrow(() => CSharpTestRunner.ReturnHttpRequestMessage(testSnippet));
        }

        [TestCase("ev@ent", IncorrectVerbatimIdentifier, TestName = "ev@ent", Description = "Incorrect Verbatim Identifier")]
        public void ShouldFailWhenReturningHttpRequestMessageWithIncorrectSnippet(string variableName, string testSnippet)
        {
            Assert.Throws<AssertionException>(() => CSharpTestRunner.ReturnHttpRequestMessage(testSnippet));
        }
    }
}
