using System;
using System.Threading;
using System.Threading.Tasks;
using AccountTransactions.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Client;

class TransactionsSenderService(IMessageSession messageSession, ILogger<TransactionsSenderService> logger) : BackgroundService
{
    private static Guid _accountId = new("AEB20C17-A983-4751-BA24-2F1DB1CAFD66");
    private readonly Random _random = new();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize the negative balance
        var balance = -100;
        for (int i = 0; i < 5; i++)
        {
            logger.LogWarning("Negative balanced detected - Publishing NegativeAccountBalance event");
            await messageSession.Publish<NegativeAccountBalance>((transferred =>
            {
                transferred.AccountId = _accountId;
                transferred.Balance = balance;
            }), cancellationToken: stoppingToken);
            balance = await RandomlyGenerateTransactionsUntilBalanceEventsOut(stoppingToken, balance);
        }
    }

    private async Task<int> RandomlyGenerateTransactionsUntilBalanceEventsOut(CancellationToken stoppingToken, int balance)
    {
        do
        {
            // Randomly send out transactions to the account
            var amount = _random.Next(150, 600);
            var isDebit = _random.Next(1, 3) != 1;
            
            if (isDebit)
            {
                balance -= amount;
                await messageSession.Publish<DebitAmountTransferred>((transferred =>
                {
                    transferred.AccountId = _accountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                logger.LogInformation($"Debit amount transferred: -{amount}, balance is now {balance}");
            }
            else
            {
                balance += amount;
                await messageSession.Publish<CreditAmountTransferred>((transferred =>
                {
                    transferred.AccountId = _accountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                logger.LogInformation($"Credit amount transferred: {amount}, balance is now {balance}");
            }

            await Task.Delay(100, stoppingToken);
        } while (balance < 0);
        
        logger.LogWarning("Balance replenished");
        return balance;
    }
}