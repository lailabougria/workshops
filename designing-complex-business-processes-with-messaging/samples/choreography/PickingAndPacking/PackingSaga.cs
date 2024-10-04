using Contracts;
using NServiceBus.Logging;
using PickingAndPacking.InternalContracts;

namespace PickingAndPacking;

public class PackingSaga : Saga<PackingSagaData>, 
    IAmStartedByMessages<PackOrder>,
    IHandleMessages<ReserveProduct>,
    IHandleMessages<ProductReserved>,
    IHandleMessages<BackOrderProduct>,
    IHandleMessages<ProductRestocked>,
    IHandleTimeouts<PackingSlaExceeded>
{
    private static readonly ILog Logger = LogManager.GetLogger<PackingPrerequisitesSaga>();
    private static TimeSpan SLA = TimeSpan.FromSeconds(3);

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PackingSagaData> mapper)
    {
        mapper.MapSaga(saga => saga.OrderId)
            .ToMessage<PackOrder>(msg => msg.OrderId)
            .ToMessage<ReserveProduct>(msg => msg.OrderId)
            .ToMessage<ProductReserved>(msg => msg.OrderId)
            .ToMessage<BackOrderProduct>(msg => msg.OrderId)
            .ToMessage<ProductRestocked>(msg => msg.OrderId);
    }
    
   public async Task Handle(PackOrder message, IMessageHandlerContext context)
   {
       Data.OrderId = message.OrderId;
       Data.ProductsToPack = message.Orderlines.Select(x => new PackingSagaData.OrderLinePackingStatus
       {
           ProductId = x.ProductId,
           Quantity = x.Quantity
       }).ToList();
       
       Logger.Info($"Initialize picking and packing for order {Data.OrderId}");
       foreach (var productToPack in Data.ProductsToPack)
       {
           await context.SendLocal(new ReserveProduct
           {
               OrderId = Data.OrderId,
               ProductId = productToPack.ProductId, 
               Quantity = productToPack.Quantity,
               SLA = SLA // indicates to stop trying to reserve stock after SLA has been exceeded
           });
       }
       
       await RequestTimeout<PackingSlaExceeded>(context, SLA);
    }

    public async Task Handle(ProductReserved message, IMessageHandlerContext context)
    {
        if (Data.SLAExceeded)
        {
            // This is to capture any events that occur after the SLA has been exceeded due to concurrency or ordering issues
            Logger.Warn($"Restock for product {message.ProductId} was received after SLA exceeded, ignoring");;
            return;
        }
        
        Data.ProductsToPack.First(x => x.ProductId == message.ProductId).PackedQuantity = message.Quantity;
        if (Data.ProductsToPack.All(x => x.IsFullyPacked))
        {
            Logger.Warn($"All products for order {Data.OrderId} have been packed!");
            await context.Publish<IOrderPacked>(packed =>
            {
                packed.OrderId = Data.OrderId;
            });
            
            if (Data.ProductRestocksInProgress.Count == 0)
            {
                Logger.Warn($"All done for order {Data.OrderId}");
                MarkAsComplete();
            }
        }
    }

    public async Task Timeout(PackingSlaExceeded state, IMessageHandlerContext context)
    {
        Logger.Warn($"Packing SLA exceeded for order {Data.OrderId}");

        var productsMissing = Data.ProductsToPack.Where(x => !x.IsFullyPacked).ToList();
        await context.Publish<IPartialOrderPacked>(partialOrder =>
        {
            partialOrder.OrderId = Data.OrderId;
            partialOrder.MissingProducts = productsMissing
                .Select(x => new KeyValuePair<Guid, int>(x.ProductId, x.Quantity - x.PackedQuantity))
                .ToDictionary();
        });
        
        Data.SLAExceeded = true;
    }

    public async Task Handle(ReserveProduct message, IMessageHandlerContext context)
    {
        var reserved = ProductStore.GetInstance().ReserveProduct(message.ProductId, message.Quantity);
        if (reserved)
        {
            Logger.Info($"Not enough stock to reserve {message.Quantity} items of product {message.ProductId} for order {message.OrderId}, requesting a back order...");
            Data.ProductRestocksInProgress.Add(message.ProductId);
            await context.SendLocal(new BackOrderProduct{
                OrderId = message.OrderId, 
                ProductId = message.ProductId, 
                Quantity = message.Quantity});
            return;
        }
        
        Logger.Info($"Reserved {message.Quantity} items of product {message.ProductId} for order {message.OrderId}");
        await context.SendLocal(new ProductReserved
        {
            OrderId = message.OrderId,
            ProductId = message.ProductId,
            Quantity = message.Quantity
        });
    }

    public Task Handle(BackOrderProduct message, IMessageHandlerContext context)
    {
        Random random = new Random();
        var restockInTime = random.Next(0, 1) == 1;

        var sendOptions = new SendOptions();
        if (!restockInTime)
            sendOptions.DelayDeliveryWith(SLA + TimeSpan.FromSeconds(1));
        
        return context.Send(new ProductRestocked
        {
            OrderId = message.OrderId,
            ProductId = message.ProductId
        }, sendOptions);
    }

    public async Task Handle(ProductRestocked message, IMessageHandlerContext context)
    {
        Logger.Info($"Restocked product {message.ProductId} for order {message.OrderId}");
        ProductStore.GetInstance().RestockProduct(message.ProductId, message.Quantity);
        Data.ProductRestocksInProgress.Remove(message.ProductId);

        if (!Data.SLAExceeded)
        {
            await context.SendLocal(new ProductReserved
            {
                OrderId = message.OrderId,
                ProductId = message.ProductId,
                Quantity = message.Quantity
            });
        }
        else if (Data.ProductRestocksInProgress.Count == 0)
        {
            // When all restocks are complete we can stop the saga as the SLA has been exceeded
            Logger.Warn($"All done for order {Data.OrderId}");
            MarkAsComplete();
        }
    }
}

public class PackingSagaData : ContainSagaData
{
    public bool SLAExceeded { get; set; }
    public Guid OrderId { get; set; }
    public List<OrderLinePackingStatus> ProductsToPack { get; set; } = new();
    public List<Guid> ProductRestocksInProgress { get; set; } = new();
    
    public class OrderLinePackingStatus
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int PackedQuantity { get; set; }
        public bool IsFullyPacked => PackedQuantity == Quantity;
    }
}
