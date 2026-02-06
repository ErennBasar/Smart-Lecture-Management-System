namespace Student.API.DTOs;

public class UpdateStudentDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Guid? AdvisorId { get; set; }
}