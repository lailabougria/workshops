namespace PickingAndPacking.InternalContracts;

public class BackOrderProduct : ICommand
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}