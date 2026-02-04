using Shared.Models;

namespace Lecture.API.Models.Entities;

public class CourseEnrollment : BaseEntity
{
    public Guid LectureId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public string? AdvisorComment { get; set; }

    public Lecture Lecture { get; set; } = null!;

}