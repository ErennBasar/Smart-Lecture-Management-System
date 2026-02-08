namespace Advisor.API.DTOs;

public class UpdateAdvisorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Specialization { get; set; } 
}