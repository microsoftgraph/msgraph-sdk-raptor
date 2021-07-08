using NUnit.Framework;
using MsGraphSDKSnippetsCompiler.Models;

namespace UnitTests
{
    public class Tests
    {
        private IdentifierReplacer idReplacer;

        [SetUp]
        public void Setup()
        {
            // identifiers.json holds sample tree constructed from V1 urls
            var identifiersJson = System.IO.File.ReadAllText("identifiers.json");
            var tree = System.Text.Json.JsonSerializer.Deserialize<IDTree>(identifiersJson);

            idReplacer = new IdentifierReplacer(tree);
        }

        [TestCase("https://graph.microsoft.com/v1.0/applications/{application-id}/owners",
                  "https://graph.microsoft.com/v1.0/applications/<application>/owners")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/<team>/channels/<team_channel>/members/<team_channel_conversationMember>")]
        [TestCase("https://graph.microsoft.com/v1.0/communications/callRecords/{callRecords.callRecord-id}?$expand=sessions($expand=segments)",
                  "https://graph.microsoft.com/v1.0/communications/callRecords/<callRecords.callRecord>?$expand=sessions($expand=segments)")]
        [TestCase("https://graph.microsoft.com/v1.0/chats/{chat-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/chats/<chat>/members/<chat_conversationMember>")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/<team>/members/<team_conversationMember>")]
        [TestCase("https://graph.microsoft.com/v1.0/education/schools/{educationSchool-id}/users",
                  "https://graph.microsoft.com/v1.0/education/schools/<educationSchool>/users")]
        public void TestIds(string snippetUrl, string expectedUrl)
        {
            var newUrl = idReplacer.ReplaceIds(snippetUrl);
            Assert.AreEqual(expectedUrl, newUrl);
        }

    }
}
