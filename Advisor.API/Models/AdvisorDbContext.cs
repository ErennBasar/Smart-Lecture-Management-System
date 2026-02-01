using Microsoft.EntityFrameworkCore;

namespace Advisor.API.Models;

public class AdvisorDbContext : DbContext
{
    public AdvisorDbContext(DbContextOptions<AdvisorDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Advisor> Advisors { get; set; }
}