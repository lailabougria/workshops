using System.Diagnostics;
using PickingAndPacking.InternalContracts;
using Contracts;

namespace PickingAndPacking;

public sealed class ProductStore
{
    private readonly SemaphoreSlim _semaphoreEIP;
    private readonly SemaphoreSlim _semaphoreCoupling;
    private readonly SemaphoreSlim _semaphoreSA;

    private readonly ProductStock _enterpriseIntegrationPatterns;
    private readonly ProductStock _balancingCoupling;
    private readonly ProductStock _softwareArchitectureThehardparts;
    
    private ProductStore()
    {
        _semaphoreCoupling = new SemaphoreSlim(1, 1);
        _semaphoreEIP = new SemaphoreSlim(1, 1);
        _semaphoreSA = new SemaphoreSlim(1, 1);
        
        _enterpriseIntegrationPatterns = new ProductStock {ProductId = ProductCatalog.EnterpriseIntegrationPatterns, Stock = 10};
        _balancingCoupling = new ProductStock {ProductId = ProductCatalog.BalancingCouplingInSoftwareDesign, Stock = 10};
        _softwareArchitectureThehardparts = new ProductStock {ProductId = ProductCatalog.SoftwareArchitectureTheHardParts, Stock = 10};
    }

    private static readonly Lazy<ProductStore> LazyInstance = new(() => new ProductStore(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ProductStore GetInstance()
    {
        return LazyInstance.Value;
    }
   
    public bool ReserveProduct(Guid productId, int quantity)
    {
        Activity? activity = Shared.PickingAndPackingSource.StartActivity("product_store.reserve_product");
        activity?.AddTag("product_id", productId);
        
        var product = GetProduct(productId);
        var semaphore = GetSemaphoreForProduct(productId);
        semaphore.Wait();
        var isReserved = false;
        
        if (product.Stock >= quantity)
        {
            product.Stock -= quantity;
            isReserved = true;
            activity?.AddEvent(new ActivityEvent("ProductReserved"));
        }
        activity?.AddEvent(new ActivityEvent("InsufficienStock"));

        semaphore.Release();
        return isReserved;
    }

    public void RestockProduct(Guid productId, int quantity)
    {
        Activity? activity = Shared.PickingAndPackingSource.StartActivity("product_store.restock_product");
        activity?.AddTag("product_id", productId);
        activity?.AddTag("restock_quantity", quantity);
        
        var product = GetProduct(productId);
        var semaphore = GetSemaphoreForProduct(productId);
        semaphore.Wait();
        product.Stock += quantity;
        semaphore.Release();
    }
    
    private ProductStock GetProduct(Guid productId)
    {
        if (productId == _enterpriseIntegrationPatterns.ProductId)
            return _enterpriseIntegrationPatterns;
        if (productId == _balancingCoupling.ProductId)
            return _balancingCoupling;
        return _softwareArchitectureThehardparts;
    }
    
    private SemaphoreSlim GetSemaphoreForProduct(Guid productId)
    {
        if (productId == _enterpriseIntegrationPatterns.ProductId)
            return _semaphoreEIP;
        if (productId == _balancingCoupling.ProductId)
            return _semaphoreCoupling;
        return _semaphoreSA;
    }
}