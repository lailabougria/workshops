using NServiceBus;

namespace Shipping.Contracts;

public class PackOrder : ICommand
{
    public string OrderId { get; set; }
}