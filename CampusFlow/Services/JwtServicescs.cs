using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CampusFlow.DTO;
using CampusFlow.Model;

namespace CampusFlow.Services
{
    public class JwtServicescs
    {
        private readonly IConfiguration _configuration;

        public JwtServicescs(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(Student student)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, student.Email)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: new List<Claim>(),
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}