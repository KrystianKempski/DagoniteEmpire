using DA_DataAccess;
using DA_Models;
using DagoniteEmpire.Helper;
using DagoniteEmpire.Service.IService;
using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace DagoniteEmpire.Service
{
    public class TokenService : ITokenService
    {
        private readonly BearerAuthenticationSettings _bearerAuthenticationSettings;

        public TokenService(BearerAuthenticationSettings bearerAuthenticationSettings)
        {
            _bearerAuthenticationSettings = bearerAuthenticationSettings;
        }

        public string GenerateToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "signalUser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerAuthenticationSettings.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _bearerAuthenticationSettings.ValidIssuer,
                audience: _bearerAuthenticationSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(500),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
