namespace Contracts;

public class OrderLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}