namespace Contracts;

public interface IOrderPacked : IEvent
{
    public Guid OrderId { get; set; }
}