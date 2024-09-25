using NServiceBus;

namespace Shipping;

class ShippingPolicyData : ContainSagaData
{
    public string OrderId { get; set; }
    public bool IsOrderPacked { get; set; }
    public bool IsOrderShipped { get; set; }
    public bool IsOrderCancelled { get; set; }
}