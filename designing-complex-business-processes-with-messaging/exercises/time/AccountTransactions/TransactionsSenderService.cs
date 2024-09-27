using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AccountTransactions.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Client;

class TransactionsSenderService(IMessageSession messageSession, ILogger<TransactionsSenderService> log) : BackgroundService
{
    public static Guid AccountId = new("AEB20C17-A983-4751-BA24-2F1DB1CAFD66");
    private Random _random = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Initialize the negative balance
            await messageSession.Publish<NegativeAccountBalance>((transferred =>
            {
                transferred.AccountId = AccountId;
                transferred.Balance = -100;
            }), cancellationToken: stoppingToken);
            
            // Randomly send out transactions to the account
            var amount = _random.Next(150, 600);
            var isDebit = _random.Next(0, 1) == 1;
            
            if (isDebit)
            {
                await messageSession.Publish<DebitAmountTransferred>((transferred =>
                {
                    transferred.AccountId = AccountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                log.LogInformation($"Debit amount transferred: {amount}");
            }
            else
            {
                await messageSession.Publish<CreditAmountTransferred>((transferred =>
                {
                    transferred.AccountId = AccountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                log.LogInformation($"Credit amount transferred: {amount}");
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}