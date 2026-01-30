using Shared.Enums;
using Shared.Models;

namespace Identity.API.Models.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole UserRole { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
}