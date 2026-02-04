using Shared.Models;

namespace Lecture.API.Models.Entities;

public class Grade : BaseEntity
{
    public Guid LectureId { get; set; }
    public Guid StudentId { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 100;
    public string? ExamType { get; set; } // Midterm, Final, Quiz, etc.
    public string? Comments { get; set; }
    public DateTime GradeDate { get; set; } = DateTime.UtcNow;

    public Lecture Lecture { get; set; } = null!;

}