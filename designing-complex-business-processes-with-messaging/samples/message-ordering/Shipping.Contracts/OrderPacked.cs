using NServiceBus;

namespace Shipping.Contracts;

public class OrderPacked : IEvent
{
    public string OrderId { get; set; }
}