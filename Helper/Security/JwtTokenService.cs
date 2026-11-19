using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FrontOfficeApplication.Helper.Security
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string UserName, string Branchcode, string UserCode, string roleId)
        {
            var jwtKey = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is not configured.");
            }

            var issuer = _config["Jwt:Issuer"];

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new InvalidOperationException(
                    "Jwt:Issuer is not configured.");
            }

            var audience = _config["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException(
                    "Jwt:Audience is not configured.");
            }

            if (string.IsNullOrWhiteSpace(Branchcode))
            {
                throw new ArgumentException(
                    "Branch code is required.",
                    nameof(Branchcode));
            }

            var claims = new[]
            {
                new Claim( ClaimTypes.Name, UserName),
                new Claim( "Branch_code", Branchcode),
                new Claim( "UserCode", UserCode),
                new Claim( "RoleId", roleId)
            };

            var key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials( key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler() .WriteToken(token);
        }
    }
}
