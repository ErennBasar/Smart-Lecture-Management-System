using Microsoft.AspNetCore.Identity;
using Shared.Enums;
using Shared.Models;

namespace Identity.API.Models.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; } = false;
}