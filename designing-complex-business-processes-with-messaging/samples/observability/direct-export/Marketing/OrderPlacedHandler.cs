using Contracts;
using NServiceBus;
using NServiceBus.Logging;

namespace Marketing;

public class OrderPaidHandler : IHandleMessages<IOrderPaid>
{
    private static readonly ILog log = LogManager.GetLogger<OrderPaidHandler>();

    public Task Handle(IOrderPaid message, IMessageHandlerContext context)
    {
        log.Info($"Verifying whether customer {message.CustomerId} is eligible for a discount code...");

        log.Info("Customer was not yet eligible for a discount code...");
        return Task.CompletedTask;
    }
}