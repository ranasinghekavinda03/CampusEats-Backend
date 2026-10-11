using CampusEats.Api.Dtos;

namespace CampusEats.Api.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);

    Task<string?> LoginAsync(LoginDto dto);
}