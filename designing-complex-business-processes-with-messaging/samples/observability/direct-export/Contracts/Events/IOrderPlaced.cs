namespace Contracts;

public interface IOrderPlaced : IEvent
{
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    List<OrderLine> OrderLines { get; set; }
}