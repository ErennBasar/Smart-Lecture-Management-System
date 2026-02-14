namespace Shared.Events;

public interface IAdvisorDeletedEvent
{
    public Guid AdvisorId { get; set; }
    public Guid UserId { get; set; }
}