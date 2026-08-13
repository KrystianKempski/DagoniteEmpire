using System.Security.Claims;
using System.Text.Json;
using DagoniteEmpire.Account.Pages;
using DagoniteEmpire.Account.Pages.Manage;
using DA_DataAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            accountGroup.MapPost("/PerformExternalLogin", (
                HttpContext context,
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromForm] string provider,
                [FromForm] string returnUrl) =>
            {
                IEnumerable<KeyValuePair<string, StringValues>> query = [
                    new("ReturnUrl", returnUrl),
                    new("Action", ExternalLogin.LoginCallbackAction)];

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/ExternalLogin",
                    QueryString.Create(query));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return TypedResults.Challenge(properties, [provider]);
            });

            accountGroup.MapPost("/Logout", async (
                ClaimsPrincipal user,
                SignInManager<ApplicationUser> signInManager,
                [FromForm] string returnUrl) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            // Public "Try baron" demo: provision an isolated cloned barony and sign in the hidden demo user.
            accountGroup.MapGet("/DemoBaron", async (
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromServices] UserManager<ApplicationUser> userManager,
                [FromServices] DA_Business.Services.Interfaces.IDemoBaronyService demoService) =>
            {
                var demoUser = await userManager.FindByNameAsync(DA_Common.SD.DemoBaronUserName);
                if (demoUser is null)
                    return Results.LocalRedirect("~/Account/Login");

                var info = await demoService.CreateSessionAsync();
                await signInManager.SignInAsync(demoUser, isPersistent: false);
                return Results.LocalRedirect($"~/demo/enter?t={info.Token}&c={info.CharacterId}");
            }).AllowAnonymous();

            // Public "Try Game Master" demo: same isolated cloned barony, signed in as the hidden GM.
            accountGroup.MapGet("/DemoGameMaster", async (
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromServices] UserManager<ApplicationUser> userManager,
                [FromServices] DA_Business.Services.Interfaces.IDemoBaronyService demoService) =>
            {
                var demoGmUser = await userManager.FindByNameAsync(DA_Common.SD.DemoGmUserName);
                if (demoGmUser is null)
                    return Results.LocalRedirect("~/Account/Login");

                var info = await demoService.CreateSessionAsync();
                await signInManager.SignInAsync(demoGmUser, isPersistent: false);
                return Results.LocalRedirect($"~/demo/enter?t={info.Token}&c={info.CharacterId}");
            }).AllowAnonymous();

            // Leave the demo: purge the session's cloned data and sign out.
            accountGroup.MapGet("/DemoExit", async (
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromServices] DA_Business.Services.Interfaces.IDemoBaronyService demoService,
                [FromQuery] string? t) =>
            {
                if (Guid.TryParse(t, out var token))
                    await demoService.EndSessionAsync(token);
                await signInManager.SignOutAsync();
                return Results.LocalRedirect("~/Account/Login");
            }).AllowAnonymous();

            // Fire-and-forget beacon sent by the browser on pagehide (tab close / navigate away):
            // mark the session near-expired so the sweeper purges it promptly if the visitor is gone.
            accountGroup.MapPost("/DemoLeave", async (
                [FromServices] DA_Business.Services.Interfaces.IDemoBaronyService demoService,
                [FromQuery] string? t) =>
            {
                if (Guid.TryParse(t, out var token))
                    await demoService.MarkLeavingAsync(token);
                return Results.Ok();
            }).AllowAnonymous();

            var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

            manageGroup.MapPost("/LinkExternalLogin", async (
                HttpContext context,
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromForm] string provider) =>
            {
                // Clear the existing external cookie to ensure a clean login process
                await context.SignOutAsync(IdentityConstants.ExternalScheme);

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/Manage/ExternalLogins",
                    QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, signInManager.UserManager.GetUserId(context.User));
                return TypedResults.Challenge(properties, [provider]);
            });

            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

            manageGroup.MapPost("/DownloadPersonalData", async (
                HttpContext context,
                [FromServices] UserManager<ApplicationUser> userManager,
                [FromServices] AuthenticationStateProvider authenticationStateProvider) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                {
                    return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

                // Only include personal data for download
                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                }

                var logins = await userManager.GetLoginsAsync(user);
                foreach (var l in logins)
                {
                    personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                }

                personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
                var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

                context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
                return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
            });

            return accountGroup;
        }
    }
}
