namespace Shared.Services;

public interface IAuthenticatedUserService
{
    Guid UserId { get; }
    string Email { get; }
    string FullName { get; }
    // İleride Role vs. lazım olursa buraya eklenecek
}