namespace Contracts;

public interface IOrderPaid : IEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
}