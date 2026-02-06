namespace Lecture.API.DTOs;

public class CreateLectureDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public Guid? AdvisorId { get; set; }
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; } = 30;
}