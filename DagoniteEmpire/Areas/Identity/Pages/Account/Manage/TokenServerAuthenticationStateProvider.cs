//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
//using Microsoft.JSInterop;
//using System.Security.Claims;
//using System.Text.Json;

//namespace DagoniteEmpire.Areas.Identity.Pages.Account.Manage
//{
//    public static class ServiceExtensions
//    {
//        public static void AddTokenAuthenticationStateProvider(this IServiceCollection services)
//        {
//            // Make the same instance accessible as both AuthenticationStateProvider and TokenAuthenticationStateProvider
//            services.AddScoped<TokenServerAuthenticationStateProvider>();
//            services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<TokenServerAuthenticationStateProvider>());
//        }

//        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
//        {
//            var payload = jwt.Split('.')[1];
//            var jsonBytes = ParseBase64WithoutPadding(payload);
//            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
//            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
//        }

//        private static byte[] ParseBase64WithoutPadding(string base64)
//        {
//            switch (base64.Length % 4)
//            {
//                case 2: base64 += "=="; break;
//                case 3: base64 += "="; break;
//            }
//            return Convert.FromBase64String(base64);
//        }
//    }
//    public class TokenServerAuthenticationStateProvider :
//                                 AuthenticationStateProvider
//    {
//        private readonly IJSRuntime _jsRuntime;

//        public TokenServerAuthenticationStateProvider(IJSRuntime jsRuntime)
//        {
//            _jsRuntime = jsRuntime;


//        }

//        public async Task<string> GetTokenAsync()
//             => await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

//        public async Task SetTokenAsync(string token)
//        {
//            if (token == null)
//            {
//                await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", "authToken");
//            }
//            else
//            {
//                await _jsRuntime.InvokeAsync<object>("localStorage.setItem", "authToken", token);
//            }

//            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//        }

//        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//        {
//            var token = await GetTokenAsync();
//            var identity = string.IsNullOrEmpty(token)
//                ? new ClaimsIdentity()
//                : new ClaimsIdentity(ServiceExtensions.ParseClaimsFromJwt(token), "jwt");
//            return new AuthenticationState(new ClaimsPrincipal(identity));
//        }
//    }
//}
