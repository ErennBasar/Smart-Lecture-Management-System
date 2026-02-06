using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Student.API.Models;

namespace Student.API.Infrastructure;

public class StudentDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StudentDbContext>
{
    public StudentDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connString = configurationRoot.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<StudentDbContext>();

        optionsBuilder.UseNpgsql(connString);

        return new StudentDbContext(optionsBuilder.Options);
    }
}