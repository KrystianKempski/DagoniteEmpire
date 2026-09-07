using System.Security.Claims;
using DA_Business.Services.Interfaces;
using DA_Models.NotificationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DagoniteEmpire.Service
{
    /// <summary>
    /// Subscription plumbing for PWA push notifications. Called by <c>wwwroot/js/push-notifications.js</c>
    /// with the Identity cookie, so every endpoint is tied to the signed-in user.
    /// </summary>
    [ApiController]
    [Route("api/push")]
    [Authorize]
    public class PushController : ControllerBase
    {
        private readonly IPushNotificationService _push;

        public PushController(IPushNotificationService push)
        {
            _push = push;
        }

        /// <summary>VAPID public key the browser needs before it can subscribe.</summary>
        [HttpGet("public-key")]
        [AllowAnonymous]
        public IActionResult GetPublicKey() =>
            Ok(new { configured = _push.IsConfigured, publicKey = _push.PublicKey });

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userId = CurrentUserId();
            if (userId is null)
                return Unauthorized();

            return Ok(new
            {
                configured = _push.IsConfigured,
                devices = await _push.CountSubscriptions(userId),
            });
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] WebPushSubscriptionDTO subscription)
        {
            var userId = CurrentUserId();
            if (userId is null)
                return Unauthorized();
            if (subscription is null || !subscription.IsValid)
                return BadRequest("Incomplete push subscription.");

            await _push.SaveSubscription(userId, subscription, Request.Headers.UserAgent.ToString());
            return Ok(new { devices = await _push.CountSubscriptions(userId) });
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
        {
            var userId = CurrentUserId();
            if (userId is null)
                return Unauthorized();
            if (string.IsNullOrWhiteSpace(request?.Endpoint))
                return BadRequest("Missing endpoint.");

            await _push.RemoveSubscription(request.Endpoint);
            return Ok(new { devices = await _push.CountSubscriptions(userId) });
        }

        /// <summary>Sends a test notification to every device of the current user.</summary>
        [HttpPost("test")]
        public async Task<IActionResult> SendTest()
        {
            var userId = CurrentUserId();
            if (userId is null)
                return Unauthorized();

            var sent = await _push.SendToUser(userId, new PushNotificationDTO
            {
                Title = "Dagonite Empire",
                Body = "Powiadomienia działają.",
                Url = "/",
                Tag = "push-test",
            });

            return Ok(new { sent });
        }

        private string? CurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public class UnsubscribeRequest
        {
            public string? Endpoint { get; set; }
        }
    }
}
