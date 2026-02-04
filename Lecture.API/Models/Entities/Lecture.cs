using Shared.Enums;
using Shared.Models;

namespace Lecture.API.Models.Entities;

public class Lecture : BaseEntity
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public Guid AdvisorId { get; set; }
    public LectureStatus Status { get; set; } = LectureStatus.NotStarted;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; } = 30;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
    public ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
    
}