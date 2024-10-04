using Contracts;
using NServiceBus;

namespace Commands;

public class PlaceOrder : ICommand
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public required List<OrderLine> OrderLines { get; set; }
}