using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Shared.Services;

public class AuthenticatedUserService : IAuthenticatedUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
        }
    }

    public string Email
    {
        get
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

            return string.IsNullOrEmpty(userEmail) ? string.Empty : userEmail;
        }
    }

    public string FullName
    {
        get
        {
            var givenName = _httpContextAccessor.HttpContext?.User?.FindFirst("given_name")?.Value ?? "";
            var familyName = _httpContextAccessor.HttpContext?.User?.FindFirst("family_name")?.Value ?? "";
            return $"{givenName} {familyName}".Trim();
        }
    }
}