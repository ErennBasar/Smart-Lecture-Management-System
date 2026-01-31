using Identity.API.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace Identity.API.Services;

public interface IAuthService
{
    Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterRequestDto request);
    Task<(bool IsSucces, string? Token, string Message)> LoginAsync(LoginRequestDto loginRequestDto);
}