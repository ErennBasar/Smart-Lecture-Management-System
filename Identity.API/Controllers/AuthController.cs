using Identity.API.DTOs;
using Identity.API.Models.Entities;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authService, UserManager<User> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
        {
            var result = await _authService.RegisterAsync(registerRequestDto);

            if (result.IsSuccess == false)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            var result = await _authService.LoginAsync(loginRequestDto);

            if (result.IsSucces)
                return Ok(new
                {
                    token = result.Token,
                    message = result.Message
                });

            return BadRequest(new
            {
                message = result.Message
            });
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId) // CreateAdvisor endpoint'i çalıştığında gelen ID'ye sahip bir kullanıcı var mı diye bu endpoint'e httpClient ile soruyor
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return NotFound("Kullanıcı bulunamadı");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                message = "Kullanıcı mevcut, bilgileri aşağıdadır",
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = roles,
                isActive = user.IsActive
            });
        }

        [HttpPut("update/{userId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return NotFound("Kullanıcı Bulunamadı...");

            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;

            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;

            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Ok(new
                {
                    Message = "Kullanıcı Users tablosunda da güncellendi."
                });

            return BadRequest(result.Errors);
        }
    }
}
