using Identity.API.DTOs;
using Identity.API.Models.Entities;
using Identity.API.Services;
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
        public async Task<IActionResult> GetAdvisorById(Guid userId)
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
    }
}
