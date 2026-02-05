using Identity.API.Models.Entities;

namespace Identity.API.Services;

public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles); 
}