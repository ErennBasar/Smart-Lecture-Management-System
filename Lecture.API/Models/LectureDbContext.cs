using Lecture.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lecture.API.Models;

public class LectureDbContext : DbContext
{
    public LectureDbContext(DbContextOptions<LectureDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Entities.Lecture> Lectures { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
    public DbSet<Grade> Grades { get; set; }
    
}