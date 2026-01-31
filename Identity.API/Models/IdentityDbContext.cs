using Identity.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Models;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
}