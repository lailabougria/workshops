using NServiceBus;

namespace Shipping.Contracts;

public class ShippingCancelled : IEvent
{
    public string OrderId { get; set; }
}