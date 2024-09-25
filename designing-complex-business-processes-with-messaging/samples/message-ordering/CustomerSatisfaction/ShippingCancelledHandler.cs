using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Contracts;

namespace CustomerSatisfaction;

public class ShippingCancelledHandler : IHandleMessages<ShippingCancelled>
{
    static ILog log = LogManager.GetLogger<ShippingCancelledHandler>();

    public Task Handle(ShippingCancelled message, IMessageHandlerContext context)
    {
        log.Info($"Shipping cancelled for OrderId = {message.OrderId} - Assigning voucher...");

        return Task.CompletedTask;
    }
}