using NServiceBus;

namespace Commands;

public class UpdateProductStock : ICommand
{
    public int Quantity { get; set; }
    public Guid ProductId { get; set; }
}