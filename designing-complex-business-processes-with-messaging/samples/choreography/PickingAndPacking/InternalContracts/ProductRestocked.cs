namespace PickingAndPacking.InternalContracts;

public class ProductRestocked: IMessage
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}