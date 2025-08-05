using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeamDesk.Models.Entities;

namespace TeamDesk.Helpers
{
    public static class JwtTokenGenerator
    {
        //public static string GenerateToken(Staff staff, IConfiguration config)
        //{
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var claims = new[]
        //    {
        //    new Claim(ClaimTypes.NameIdentifier, staff.StaffId.ToString()),
        //    new Claim(ClaimTypes.Email, staff.Email),
        //    new Claim(ClaimTypes.Name, staff.FullName),
        //    new Claim(ClaimTypes.Role, staff.Role)
        //};

        //    var token = new JwtSecurityToken(
        //        config["Jwt:Issuer"],
        //        config["Jwt:Audience"],
        //        claims,
        //        expires: DateTime.UtcNow.AddDays(1),
        //        signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }

}
