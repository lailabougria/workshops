using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Commands;
using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Client;

class MessageSenderService(IMessageSession messageSession, ILogger<MessageSenderService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Sending 5 orders");
        Random random = new();
        for (int i = 0; i < 5; i++)
        {
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            await messageSession.Send(new PlaceOrder
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderLines = new List<OrderLine>
                {
                    new() { ProductId = ProductCatalog.EnterpriseIntegrationPatterns, Quantity = random.Next(1, 5)},
                    new() { ProductId = ProductCatalog.BalancingCouplingInSoftwareDesign, Quantity = random.Next(1, 5)},
                    new() { ProductId = ProductCatalog.SoftwareArchitectureTheHardParts, Quantity = random.Next(1, 5)},
                }
            }, cancellationToken: stoppingToken);
            log.LogInformation($"Placing order {orderId} for customer {customerId}");
            
            // Artificial delay for the payment
            await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
            await messageSession.Send(new ChargeOrder
            {
                OrderId = orderId,
                CustomerId = customerId,
                Total = random.Next(15, 53)*i
            }, cancellationToken: stoppingToken);
            log.LogInformation($"Charging order {orderId} for customer {customerId}");
            
            await Task.Delay(i+1 * 100, stoppingToken);
        }
        log.LogInformation("5 orders sent");
    }
}