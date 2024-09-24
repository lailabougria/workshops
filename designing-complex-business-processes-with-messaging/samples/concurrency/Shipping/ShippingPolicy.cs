using System;
using Messages;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using Shipping.Contracts;

namespace Shipping;

class ShippingPolicy : Saga<ShippingPolicyData>, IAmStartedByMessages<OrderPlaced>,
    IHandleMessages<PackOrder>,
    IHandleMessages<ShipOrder>,
    IHandleMessages<OrderPacked>,
    IHandleTimeouts<ShippingSLA>
{
    static ILog _logger = LogManager.GetLogger<ShippingPolicy>();
    Random _random = new Random();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
    {
        mapper.MapSaga(sagaData => sagaData.OrderId)
            .ToMessage<OrderPlaced>(message => message.OrderId)
            .ToMessage<PackOrder>(message => message.OrderId)
            .ToMessage<OrderPacked>(message => message.OrderId)
            .ToMessage<ShipOrder>(message => message.OrderId);
    }

    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        Data.OrderId = message.OrderId;
        _logger.Info($"Order [{Data.OrderId}] - Order Placed. Start packing.");
            
        // Set a timeout to enforce the shipment SLA (in seconds, to speed things up)
        await RequestTimeout<ShippingSLA>(context, TimeSpan.FromSeconds(2));
            
        await context.SendLocal(new PackOrder { OrderId = Data.OrderId });
    }

    public Task Timeout(ShippingSLA state, IMessageHandlerContext context)
    {
        if (!Data.IsOrderShipped)
        {
            _logger.Info($"Order [{Data.OrderId}] - SLA expired. Cancelling order.");
            return context.Publish<ShippingCancelled>(cancelled => cancelled.OrderId = Data.OrderId);
        }
        return Task.CompletedTask;
    }

    public async Task Handle(PackOrder message, IMessageHandlerContext context)
    {
        _logger.Info($"Order [{message.OrderId}] - Started packing.");
            
        // Simulate a random delay in packing
        await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 4)), context.CancellationToken);
            
        Data.IsOrderPacked = true;
        await context.Publish<OrderPacked>(orderPacked => orderPacked.OrderId = Data.OrderId);
    }

    public Task Handle(ShipOrder message, IMessageHandlerContext context)
    {
        _logger.Info($"Order [{message.OrderId}] - Successfully shipped.");
        Data.IsOrderShipped = true;
        return context.Publish<OrderShipped>(orderShipped => orderShipped.OrderId = Data.OrderId);
    }

    public Task Handle(OrderPacked message, IMessageHandlerContext context)
    {
        _logger.Info($"Order [{message.OrderId}] Order Packed - Start shipping.");
        return context.SendLocal(new ShipOrder { OrderId = Data.OrderId });
    }
}