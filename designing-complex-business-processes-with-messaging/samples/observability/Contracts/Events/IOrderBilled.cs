namespace Contracts;

public interface IOrderBilled : IEvent
{
    Guid CustomerId { get; set; }
    Guid OrderId { get; set; }
}