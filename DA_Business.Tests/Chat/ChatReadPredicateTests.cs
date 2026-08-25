using Xunit;

namespace DA_Business.Tests.Chat;

/// <summary>
/// Documents the mark-as-read predicate used by ChatManager.MakeMessageRedAsync.
/// </summary>
public class ChatReadPredicateTests
{
    private static bool ShouldMarkRead(string fromUserId, string toUserId, bool isRead, string contactId, string currentUserId) =>
        fromUserId == contactId && toUserId == currentUserId && !isRead;

    [Fact]
    public void Marks_only_unread_inbound_from_contact()
    {
        Assert.True(ShouldMarkRead("contact", "me", isRead: false, contactId: "contact", currentUserId: "me"));
        Assert.False(ShouldMarkRead("contact", "me", isRead: true, contactId: "contact", currentUserId: "me"));
        Assert.False(ShouldMarkRead("me", "contact", isRead: false, contactId: "contact", currentUserId: "me"));
        Assert.False(ShouldMarkRead("other", "me", isRead: false, contactId: "contact", currentUserId: "me"));
    }

    [Fact]
    public void Old_operator_precedence_bug_would_match_all_inbound_from_contact()
    {
        // Legacy: (from==contact && to==me) || (from==me && to==contact && !read)
        // That marked already-read inbound messages as well.
        static bool LegacyBug(string from, string to, bool isRead, string contact, string me) =>
            (from == contact && to == me) || (from == me && to == contact && !isRead);

        Assert.True(LegacyBug("contact", "me", isRead: true, "contact", "me"));
        Assert.False(ShouldMarkRead("contact", "me", isRead: true, "contact", "me"));
    }
}
