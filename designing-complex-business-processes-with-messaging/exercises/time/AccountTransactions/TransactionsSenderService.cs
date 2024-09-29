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
    private static Guid _accountId = Guid.NewGuid();
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
                transferred.BalanceTimestamp = DateTime.UtcNow;
            }), cancellationToken: stoppingToken);
            
            await Task.Delay(500, stoppingToken);
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
                balance += -amount;
                await messageSession.Publish<DebitAmountTransferred>((transferred =>
                {
                    transferred.AccountId = _accountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                logger.LogInformation($"Debit amount transferred: -{amount} - Publishing DebitAmountTransferred event");
            }
            else
            {
                balance += amount;
                await messageSession.Publish<CreditAmountTransferred>((transferred =>
                {
                    transferred.AccountId = _accountId;
                    transferred.Amount = amount;
                }), cancellationToken: stoppingToken);
                logger.LogInformation($"Credit amount transferred: {amount} - Publishing CreditAmountTransferred event");
            }
            
            await Task.Delay(_random.Next(2, 7)*100, stoppingToken);
        } while (balance < 0);
        
        return balance;
    }
}