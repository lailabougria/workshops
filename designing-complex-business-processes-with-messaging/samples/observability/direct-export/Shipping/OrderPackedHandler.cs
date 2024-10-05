using Contracts;
using NServiceBus.Logging;

namespace Shipping;

public class OrderPackedHandler : IHandleMessages<IOrderPacked>, IHandleMessages<IPartialOrderPacked>
{
    private static readonly ILog Logger = LogManager.GetLogger<OrderPackedHandler>();

    public async Task Handle(IOrderPacked message, IMessageHandlerContext context)
    {
        await Ship(context, message.OrderId);
    }

    public async Task Handle(IPartialOrderPacked message, IMessageHandlerContext context)
    {
        await Ship(context, message.OrderId);
    }

    public async Task Ship(IMessageHandlerContext context, Guid orderId)
    {
        Logger.Info($"Shipping order '{orderId}'...");

        await context.Publish<IOrderShipped>(orderShipped =>
        {
            orderShipped.OrderId = orderId;
        });

        Logger.Info($"Order '{orderId}' shipped.");
    }
}