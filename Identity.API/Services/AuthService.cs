using Identity.API.DTOs;
using Identity.API.Models;
using Identity.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IdentityDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterRequestDto request)
    {
        if (await _dbContext.Users.AnyAsync(e => e.Email == request.Email))
        {
            
            return (false, "Bu Email adresi zaten var.");
        }

        var user = new User()
        {
            Email = request.Email,
            Password = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserRole = request.UserRole,
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return (true, "Kayıt Başarılı");
    }

    public async Task<(bool IsSucces, string? Token, string Message)> LoginAsync(LoginRequestDto loginRequestDto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDto.Email);
        
        if (user == null)
        {
            return (false, null, "Kullanıcı bulunamadı");
        }

        var passwordValid = _passwordHasher.VerifyPassword(loginRequestDto.Password, user.Password);

        if (passwordValid)
            return (true, "Dummy_Token", "Giriş Başarılı");

        return (false, null, "Şifre uyuşmuyor");
    }
}