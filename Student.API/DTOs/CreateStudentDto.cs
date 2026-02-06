namespace Student.API.DTOs;

public class CreateStudentDto
{
    // Identity tarafındaki kullanıcı ile eşleşmek için 
    public Guid UserId { get; set; }

    // Öğrencinin bağlı olduğu Danışman Hoca 
    public Guid AdvisorId { get; set; }

    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; } 
}