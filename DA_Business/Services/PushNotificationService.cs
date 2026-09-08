using System.Net;
using System.Text.Json;
using DA_Business.Services.Interfaces;
using DA_Common.Notifications;
using DA_DataAccess.Data;
using DA_DataAccess.Notifications;
using DA_Models.NotificationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace DA_Business.Services
{
    /// <inheritdoc cref="IPushNotificationService"/>
    public class PushNotificationService : IPushNotificationService
    {
        private static readonly JsonSerializerOptions PayloadJson = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IDbContextFactory<ApplicationDbContext> _db;
        private readonly WebPushOptions _options;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly WebPushClient _client = new();

        public PushNotificationService(
            IDbContextFactory<ApplicationDbContext> db,
            IOptions<WebPushOptions> options,
            ILogger<PushNotificationService> logger)
        {
            _db = db;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _options.IsConfigured;

        public string PublicKey => _options.IsConfigured ? _options.PublicKey : string.Empty;

        public async Task SaveSubscription(string userId, WebPushSubscriptionDTO subscription, string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userId) || subscription is null || !subscription.IsValid)
                return;

            using var ctx = await _db.CreateDbContextAsync();
            var existing = await ctx.WebPushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);

            if (existing is null)
            {
                ctx.WebPushSubscriptions.Add(new WebPushSubscription
                {
                    UserId = userId,
                    Endpoint = subscription.Endpoint,
                    P256dh = subscription.P256dh,
                    Auth = subscription.Auth,
                    TopicsJson = SerializeTopics(subscription.Topics),
                    UserAgent = Truncate(userAgent, 400),
                    CreatedUtc = DateTime.UtcNow,
                });
            }
            else
            {
                // The browser may hand the same endpoint to a different account after re-login.
                existing.UserId = userId;
                existing.P256dh = subscription.P256dh;
                existing.Auth = subscription.Auth;
                existing.UserAgent = Truncate(userAgent, 400);
                // Only overwrite preferences when the client explicitly sent a list.
                if (subscription.Topics is not null)
                    existing.TopicsJson = SerializeTopics(subscription.Topics);
            }

            await ctx.SaveChangesAsync();
        }

        public async Task RemoveSubscription(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return;

            using var ctx = await _db.CreateDbContextAsync();
            var rows = await ctx.WebPushSubscriptions
                .Where(s => s.Endpoint == endpoint)
                .ToListAsync();
            if (rows.Count == 0)
                return;

            ctx.WebPushSubscriptions.RemoveRange(rows);
            await ctx.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<string>?> GetTopics(string userId, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(endpoint))
                return null;

            using var ctx = await _db.CreateDbContextAsync();
            var row = await ctx.WebPushSubscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.Endpoint == endpoint)
                .Select(s => new { s.Id, s.TopicsJson })
                .FirstOrDefaultAsync();

            if (row is null)
                return null;

            return ParseTopicsOrAll(row.TopicsJson);
        }

        public async Task<bool> SaveTopics(string userId, string endpoint, IEnumerable<string>? topics)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(endpoint))
                return false;

            using var ctx = await _db.CreateDbContextAsync();
            var row = await ctx.WebPushSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint);
            if (row is null)
                return false;

            row.TopicsJson = SerializeTopics(topics);
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountSubscriptions(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            using var ctx = await _db.CreateDbContextAsync();
            return await ctx.WebPushSubscriptions.CountAsync(s => s.UserId == userId);
        }

        public Task<int> SendToUser(string userId, PushNotificationDTO payload, string? topic = null) =>
            SendToUsers(new[] { userId }, payload, topic);

        public async Task<int> SendToUsers(
            IEnumerable<string> userIds,
            PushNotificationDTO payload,
            string? topic = null)
        {
            if (!IsConfigured || payload is null)
                return 0;

            var targets = (userIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (targets.Count == 0)
                return 0;

            List<WebPushSubscription> devices;
            using (var ctx = await _db.CreateDbContextAsync())
            {
                devices = await ctx.WebPushSubscriptions
                    .Where(s => targets.Contains(s.UserId))
                    .ToListAsync();
            }

            var wanted = NotificationTopic.Normalize(topic);
            if (wanted is not null)
                devices = devices.Where(d => WantsTopic(d.TopicsJson, wanted)).ToList();

            if (devices.Count == 0)
                return 0;

            var json = JsonSerializer.Serialize(payload, PayloadJson);
            var vapid = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);

            var delivered = new List<int>();
            var gone = new List<int>();

            foreach (var device in devices)
            {
                try
                {
                    await _client.SendNotificationAsync(
                        new PushSubscription(device.Endpoint, device.P256dh, device.Auth),
                        json,
                        vapid);
                    delivered.Add(device.Id);
                }
                catch (WebPushException ex) when (
                    ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Gone)
                {
                    // The user uninstalled the PWA or revoked permission — forget the device.
                    gone.Add(device.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Web push delivery failed for subscription {Id}.", device.Id);
                }
            }

            if (delivered.Count > 0 || gone.Count > 0)
                await UpdateDeliveryStateAsync(delivered, gone);

            return delivered.Count;
        }

        private async Task UpdateDeliveryStateAsync(List<int> delivered, List<int> gone)
        {
            try
            {
                using var ctx = await _db.CreateDbContextAsync();

                if (gone.Count > 0)
                {
                    await ctx.WebPushSubscriptions
                        .Where(s => gone.Contains(s.Id))
                        .ExecuteDeleteAsync();
                }

                if (delivered.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    await ctx.WebPushSubscriptions
                        .Where(s => delivered.Contains(s.Id))
                        .ExecuteUpdateAsync(u => u.SetProperty(s => s.LastSentUtc, now));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not update web push subscription state.");
            }
        }

        private static bool WantsTopic(string? topicsJson, string topic)
        {
            // Legacy rows with no preference stored still receive everything.
            if (string.IsNullOrWhiteSpace(topicsJson))
                return true;

            try
            {
                var topics = JsonSerializer.Deserialize<List<string>>(topicsJson);
                // Explicit empty list = mute every category on this device.
                if (topics is null)
                    return true;

                return topics.Any(t => string.Equals(t, topic, StringComparison.OrdinalIgnoreCase));
            }
            catch (JsonException)
            {
                return true;
            }
        }

        /// <summary>
        /// Normalizes and stores the opted-in list. Null input leaves the column null
        /// ("every topic"); an empty input stores <c>[]</c> so the device stays silent.
        /// </summary>
        internal static string? SerializeTopics(IEnumerable<string>? topics)
        {
            if (topics is null)
                return null;

            var normalized = topics
                .Select(NotificationTopic.Normalize)
                .Where(t => t is not null)
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return JsonSerializer.Serialize(normalized);
        }

        internal static IReadOnlyList<string> ParseTopicsOrAll(string? topicsJson)
        {
            if (string.IsNullOrWhiteSpace(topicsJson))
                return NotificationTopic.All;

            try
            {
                var topics = JsonSerializer.Deserialize<List<string>>(topicsJson);
                if (topics is null)
                    return NotificationTopic.All;

                return topics
                    .Select(NotificationTopic.Normalize)
                    .Where(t => t is not null)
                    .Cast<string>()
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
            catch (JsonException)
            {
                return NotificationTopic.All;
            }
        }

        private static string? Truncate(string? text, int max)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            var t = text.Trim();
            return t.Length <= max ? t : t[..max];
        }
    }
}
