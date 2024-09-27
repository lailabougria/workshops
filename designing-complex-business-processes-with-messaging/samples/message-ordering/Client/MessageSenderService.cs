﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Client;

class MessageSenderService(IMessageSession messageSession, ILogger<MessageSenderService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Sending 10 orders");
        for (int i = 0; i < 10; i++)
        {
            var orderId = Guid.NewGuid().ToString();
            await messageSession.Send(new PlaceOrder { OrderId = orderId }, cancellationToken: stoppingToken);
            log.LogInformation("Order sent: {OrderId}", orderId);
            await Task.Delay(i * 100, stoppingToken);
        }
        log.LogInformation("10 orders sent");
    }
}