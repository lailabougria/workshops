using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using Shipping.Contracts;

namespace Payments;

public class ShippingCancelledHandler : IHandleMessages<ShippingCancelled>
{
    static ILog log = LogManager.GetLogger<ShippingCancelledHandler>();

    public Task Handle(ShippingCancelled message, IMessageHandlerContext context)
    {
        log.Info($"Shipping cancelled for OrderId = {message.OrderId} - Refunding credit card...");

        return Task.CompletedTask;
    }
}