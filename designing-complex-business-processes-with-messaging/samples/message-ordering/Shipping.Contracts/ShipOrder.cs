using NServiceBus;

namespace Shipping.Contracts;

public class ShipOrder : ICommand
{
    public string OrderId { get; set; }
}