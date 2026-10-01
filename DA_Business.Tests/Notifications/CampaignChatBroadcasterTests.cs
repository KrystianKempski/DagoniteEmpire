using DA_Business.Services;
using DA_Models.ChatModels;

namespace DA_Business.Tests.Notifications;

public class CampaignChatBroadcasterTests
{
    [Fact]
    public void Publish_ReachesEachCampaignTheCharacterSubscribedTo()
    {
        var bus = new CampaignChatBroadcaster();
        CampaignChatMessageDTO? fromOne = null;
        CampaignChatMessageDTO? fromTwo = null;

        using var subOne = bus.Subscribe(1, 10, msg =>
        {
            fromOne = msg;
            return Task.CompletedTask;
        });
        using var subTwo = bus.Subscribe(2, 10, msg =>
        {
            fromTwo = msg;
            return Task.CompletedTask;
        });

        bus.Publish(new CampaignChatMessageDTO
        {
            Id = 1,
            CampaignId = 2,
            SenderCharacterId = 99,
            RecipientCharacterId = null,
        });

        Assert.Null(fromOne);
        Assert.NotNull(fromTwo);
        Assert.Equal(2, fromTwo!.CampaignId);
    }
}
