namespace PickingAndPacking.InternalContracts;

public class ReserveProduct : ICommand
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public TimeSpan SLA { get; set; }
    public Guid OrderId { get; set; }
}