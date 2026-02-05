using System.Security.Claims;
using Identity.API.DTOs;
using Identity.API.Models;
using Identity.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        
        if (existingUser != null)
        {
            return (false, "Bu Email adresi zaten kayıtlı.");
        }
        
        var newUser = new User()
        {
            UserName = request.Email, // Identity UserName ister, Email'i kullanıyoruz
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        
        var result = await _userManager.CreateAsync(newUser, request.Password);

        if (!result.Succeeded)
        {
            var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Kayıt Başarısız: {errorMsg}");
        }

        //  Rol Ataması (Veritabanında bu rolün 'AspNetRoles' tablosunda olması şart!)
        await _userManager.AddToRoleAsync(newUser, request.UserRole.ToString());

        return (true, "Kayıt Başarılı");
    }

    public async Task<(bool IsSucces, string? Token, string Message)> LoginAsync(LoginRequestDto loginRequestDto)
    {
        var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
        
        if (user == null)
        {
            return (false, null, "Kullanıcı bulunamadı");
        }

        // 2. Şifreyi Kontrol Et (Identity kendi hash mekanizmasını kullanır)
        var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

        if (!passwordValid) 
        {
            return (false, null, "Şifre veya Email hatalı.");
        }
        
        // 3. Rolleri Çek (Identity Tablosundan)
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Token Üret 
        var token = _tokenService.GenerateToken(user, roles);

        return (true, token, "Giriş Başarılı");

    }
}