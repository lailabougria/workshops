using Contracts;
using NServiceBus.Logging;
using PickingAndPacking.InternalContracts;

namespace PickingAndPacking;

public class PackingPrerequisitesSaga : Saga<PackingPrerequisitesSagaData>, 
    IAmStartedByMessages<IOrderPlaced>,
    IAmStartedByMessages<IOrderPaid>
{
    private static readonly ILog Logger = LogManager.GetLogger<PackingPrerequisitesSaga>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PackingPrerequisitesSagaData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<IOrderPlaced>(msg => msg.OrderId)
            .ToMessage<IOrderPaid>(msg => msg.OrderId);
    }
    
    public async Task Handle(IOrderPlaced message, IMessageHandlerContext context)
    {
        Data.OrderId = message.OrderId;
        Data.OrderPlaced = true;
        Data.Orderlines = message.OrderLines;
        
        Logger.Info($"Order {Data.OrderId} placed");
        
        if (Data.OrderPaid)
        {
            Logger.Info($"The order {Data.OrderId} was both placed and paid for, time to pack the order!");
            await PackOrder(context);
        }
    }
    
    public async Task Handle(IOrderPaid message, IMessageHandlerContext context)
    {
        Data.OrderId = message.OrderId;
        Data.OrderPaid = true;
        
        Logger.Info($"Order {Data.OrderId} paid");
        
        if (Data.OrderPlaced)
        {
            Logger.Info($"The order {Data.OrderId} was both placed and paid for, time to pack the order!");
            await PackOrder(context);
        }
    }

    private async Task PackOrder(IMessageHandlerContext context)
    {
        await context.SendLocal(new PackOrder { OrderId = Data.OrderId, Orderlines = Data.Orderlines});
        MarkAsComplete();
    }
}

public class PackingPrerequisitesSagaData : ContainSagaData
{
    public Guid OrderId { get; set; }
    public bool OrderPaid { get; set; }
    public bool OrderPlaced { get; set; }
    public List<OrderLine> Orderlines { get; set; } = new();
}