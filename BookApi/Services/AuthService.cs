using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookApi.Models.Dto;
using Microsoft.IdentityModel.Tokens;

namespace BookApi.Services
{
	public class AuthService : IAuthService
	{
		private readonly IConfiguration _config;

		public AuthService(IConfiguration config)
		{
			_config = config;
		}

		public async Task<string> LoginAsync(LoginDto dto)
		{
			if (dto.Email != "studydotnet@yopmail.com" || dto.Password != "Abc12345@") 
				throw new UnauthorizedAccessException("Invalid credentials");

			var claims = new[]
			{
			new Claim(ClaimTypes.NameIdentifier, "1"),
			new Claim(ClaimTypes.Email, dto.Email),
			new Claim(ClaimTypes.Role, "Admin")
		};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _config["Jwt:Issuer"],
				audience: _config["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(60),
				signingCredentials: creds
			);

			return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
		}
	}
}
