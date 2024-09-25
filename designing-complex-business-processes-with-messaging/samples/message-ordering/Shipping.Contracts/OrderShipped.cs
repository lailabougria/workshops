using NServiceBus;

namespace Shipping.Contracts;

public class OrderShipped : IEvent
{
    public string OrderId { get; set; }
}