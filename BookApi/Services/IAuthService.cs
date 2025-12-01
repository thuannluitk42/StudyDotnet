using BookApi.Models.Dto;

namespace BookApi.Services
{
	public interface IAuthService
	{
		Task<AuthResponse> LoginAsync(LoginDto dto);
		Task<AuthResponse> RefreshTokenAsync(string refreshToken);
		Task LogoutAsync(string refreshToken);
	}
}
