namespace Contracts;

public interface IStockUpdated : IEvent
{
    string ProductId { get; set; }
}