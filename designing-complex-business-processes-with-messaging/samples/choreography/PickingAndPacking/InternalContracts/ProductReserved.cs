namespace PickingAndPacking.InternalContracts;

public class ProductReserved : IMessage
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
}