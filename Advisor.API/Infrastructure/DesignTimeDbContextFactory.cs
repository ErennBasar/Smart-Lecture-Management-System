using Advisor.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace Advisor.API.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdvisorDbContext>
{
    public AdvisorDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connString = configurationRoot.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<AdvisorDbContext>();
        
        optionsBuilder.UseNpgsql(connString);

        return new AdvisorDbContext(optionsBuilder.Options);
    }
}