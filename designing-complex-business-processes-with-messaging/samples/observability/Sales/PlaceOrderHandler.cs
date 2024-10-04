using Commands;
using Contracts;
using NServiceBus.Logging;

namespace Sales;

public class PlaceOrderHandler : IHandleMessages<PlaceOrder>
{
    private static readonly ILog Logger = LogManager.GetLogger<PlaceOrderHandler>();

    public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        Logger.Info($"Received order '{message.OrderId}', storing...");
        await context.Publish<IOrderPlaced>(orderPlaced =>
        {
            orderPlaced.OrderId = message.OrderId;
            orderPlaced.CustomerId = message.CustomerId;
            orderPlaced.OrderLines = message.OrderLines;
        });
    }
}