namespace Advisor.API.DTOs;

public class CreateAdvisorDto
{
    public Guid UserId { get; set; } // Identity tarafındaki GUID
    // public string FirstName { get; set; } = string.Empty;
    // public string LastName { get; set; } = string.Empty;
    //public string Email { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty; 
    public string? Department { get; set; }
    public string? Specialization { get; set; } 
}