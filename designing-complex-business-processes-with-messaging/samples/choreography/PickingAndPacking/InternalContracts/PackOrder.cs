using Contracts;

namespace PickingAndPacking.InternalContracts;

public class PackOrder : ICommand
{
    public Guid OrderId { get; set; }
    public IEnumerable<OrderLine> Orderlines { get; set; } = new List<OrderLine>();
}