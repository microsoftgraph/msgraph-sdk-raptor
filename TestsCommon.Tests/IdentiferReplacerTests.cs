using NUnit.Framework;

namespace TestsCommon.Tests
{
    public class Tests
    {
        private IdentifierReplacer idReplacer;

        [SetUp]
        public void Setup()
        {
            var tree = new IDTree(null)
            {
                ["application"] = new IDTree("<application>"),
                ["team"] = new IDTree("<team>")
                {
                    ["channel"] = new IDTree("<team_channel>")
                    {
                        ["conversationMember"] = new IDTree("<team_channel_conversationMember>")
                    },
                    ["conversationMember"] = new IDTree("<team_conversationMember>")
                },
                ["chat"] = new IDTree("<chat>")
                {
                    ["conversationMember"] = new IDTree("<chat_conversationMember>")
                },
                ["callRecords.callRecord"] = new IDTree("<callRecords.callRecord>")
            };

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
        public void TestIds(string snippetUrl, string expectedUrl)
        {
            var newUrl = idReplacer.ReplaceIds(snippetUrl);
            Assert.AreEqual(expectedUrl, newUrl);
        }
    }
}
