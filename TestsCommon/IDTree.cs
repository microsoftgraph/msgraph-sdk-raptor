using System.Collections.Generic;

namespace TestsCommon
{
    /// <summary>
    /// Tree data structure to represent ID placeholders in graph URLs.
    ///
    /// Example URLs:
    ///   https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}
    ///   https://graph.microsoft.com/v1.0/communications/callRecords/{callRecords.callRecord-id}?$expand=sessions($expand=segments)
    ///   https://graph.microsoft.com/v1.0/chats/{chat-id}/members/{conversationMember-id}
    ///
    /// Corresponding tree representation:
    ///
    ///                                 root
    ///                               /  |  \
    ///                              /   |   \
    ///                             /    |    \
    ///                         chat   team   callRecords.callRecord
    ///                          /       |
    ///         conversationMember    channel
    ///                                  |
    ///                             conversationMember
    /// </summary>
    public class IDTree : Dictionary<string, IDTree>
    {
        public string Value { get; set; }
        public IDTree(string value) { Value = value; }
    }
}

