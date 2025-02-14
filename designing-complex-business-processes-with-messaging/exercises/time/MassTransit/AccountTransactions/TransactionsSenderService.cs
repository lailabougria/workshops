using System;
using System.Threading;
using System.Threading.Tasks;
using AccountTransactions.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Client;

class TransactionsSenderService(IPublishEndpoint publishEndpoint, ILogger<TransactionsSenderService> logger) : BackgroundService
{
    private static Guid _accountId = Guid.NewGuid();
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Uncomment when there's a handler for the below message
        // Initialize the interest period
        // await sendEndpoint.Send<StartInterestPeriod>(new
        // {
        //     AccountId = _accountId,
        //     StartDate = new DateTime(DateTime.Today.Year, 1, 1)
        // }, cancellationToken: stoppingToken);
        
        // Initialize the negative balance
        var balance = -100;
        for (int i = 0; i < 5; i++)
        {
            logger.LogWarning("Negative balanced detected - Publishing NegativeAccountBalance event");
            await publishEndpoint.Publish<NegativeAccountBalance>(new
            {
                AccountId = _accountId,
                Balance = balance,
                BalanceTimestamp = DateTime.UtcNow
            }, cancellationToken: stoppingToken);

            await Task.Delay(500, stoppingToken);
            balance = await RandomlyGenerateTransactionsUntilBalanceEventsOut(stoppingToken, balance);
        }
        
        logger.LogInformation("Done generating transactions");
    }

    private async Task<int> RandomlyGenerateTransactionsUntilBalanceEventsOut(CancellationToken stoppingToken, int balance)
    {
        do
        {
            // Randomly send out transactions to the account
            var amount = _random.Next(15, 40) * 10;
            var isDebit = _random.Next(1, 3) != 1;

            if (isDebit)
            {
                balance += -amount;
                await publishEndpoint.Publish<DebitAmountTransferred>(new
                {
                    AccountId = _accountId,
                    Amount = amount
                }, cancellationToken: stoppingToken);
                logger.LogInformation($"Debit amount transferred: -{amount} - Publishing DebitAmountTransferred event");
            }
            else
            {
                balance += amount;
                await publishEndpoint.Publish<CreditAmountTransferred>(new
                {
                    AccountId = _accountId,
                    Amount = amount
                }, cancellationToken: stoppingToken);
                logger.LogInformation($"Credit amount transferred: {amount} - Publishing CreditAmountTransferred event");
            }

            await Task.Delay(_random.Next(2, 7)*100, stoppingToken);
        } while (balance < 0);

        return balance;
    }
}