using Contracts;
using NServiceBus;

namespace Commands;

public class ChargeOrder : ICommand
{
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Total { get; set; }
}