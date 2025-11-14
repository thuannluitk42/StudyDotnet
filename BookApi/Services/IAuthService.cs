using BookApi.Models.Dto;

namespace BookApi.Services
{
	public interface IAuthService
	{
		Task<string> LoginAsync(LoginDto dto);
	}
}
