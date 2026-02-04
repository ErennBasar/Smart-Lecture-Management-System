using Lecture.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lecture.API.Infrastructure;

public class LectureDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LectureDbContext>
{
    public LectureDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connString = configurationRoot.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<LectureDbContext>();
        optionsBuilder.UseNpgsql(connString);

        return new LectureDbContext(optionsBuilder.Options);

    }
}