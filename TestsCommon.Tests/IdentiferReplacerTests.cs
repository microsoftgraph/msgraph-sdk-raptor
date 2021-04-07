using NUnit.Framework;

namespace TestsCommon.Tests
{
    public class Tests
    {
        private IdentifierReplacer idReplacer;

        [SetUp]
        public void Setup()
        {
            var ids = new IDTree(null)
            {
                ["application"] = new IDTree("<application>"),
                ["team"] = new IDTree("<team>")
                {
                    ["channel"] = new IDTree("<team_channel>")
                    {
                        ["conversationMember"] = new IDTree("<team_channel_conversationMember>")
                    }
                }
            };

            idReplacer = new IdentifierReplacer(ids);
        }

        [TestCase("https://graph.microsoft.com/v1.0/applications/{application-id}/owners",
                  "https://graph.microsoft.com/v1.0/applications/<application>/owners")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/<team>/channels/<team_channel>/members/<team_channel_conversationMember>")]
        public void TestIds(string snippetUrl, string expectedUrl)
        {
            var newUrl = idReplacer.ReplaceIds(snippetUrl);
            Assert.AreEqual(expectedUrl, newUrl);
        }
    }
}
