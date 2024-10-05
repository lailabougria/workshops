namespace Contracts;

public interface IProductRestocked : IEvent
{
    string ProductId { get; set; }
}