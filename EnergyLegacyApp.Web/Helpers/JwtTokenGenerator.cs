using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;

using System.Text;

namespace EnergyLegacyApp.Web.Helpers

{
    public class JwtTokenGenerator

    {

        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)

        {

            _config = config;

        }

        public string GenerateToken(string username, string role)

        {

            var jwtSettings = _config.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]

            {

                new Claim(ClaimTypes.Name, username),

                new Claim(ClaimTypes.Role, role)

            };

            var token = new JwtSecurityToken(

                issuer: jwtSettings["Issuer"],

                audience: jwtSettings["Audience"],

                claims: claims,

                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),

                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

    }

}

