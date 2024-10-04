using Commands;
using Contracts;
using NServiceBus.Logging;

namespace Finance;

public class ChargeOrderHandler : IHandleMessages<ChargeOrder>
{
    private static readonly ILog Logger = LogManager.GetLogger<ChargeOrderHandler>();

    public async Task Handle(ChargeOrder message, IMessageHandlerContext context)
    {
        Logger.Info($"Charging credit card for order '{message.OrderId}'...");

        await context.Publish<IOrderPaid>(orderCharged =>
        {
            orderCharged.OrderId = message.OrderId;
        });

        Logger.Info($"Order '{message.OrderId}' was successfully charged to the credit card.");
    }
}

public class PartialOrderPackedHandler : IHandleMessages<IPartialOrderPacked>
{
    private static readonly ILog Logger = LogManager.GetLogger<PartialOrderPackedHandler>();
    
    public Task Handle(IPartialOrderPacked message, IMessageHandlerContext context)
    {
        Logger.Info($"Part of order '{message.OrderId}' couldn't be packed, issuing a partial refund.");
        return Task.CompletedTask;
    }
}