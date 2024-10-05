using System;
using System.Threading.Tasks;
using AccountTransactions.Contracts;
using NServiceBus;
using NServiceBus.Logging;

namespace AccountTransactions;

public class AccountInterestPolicy : Saga<AccountInterestPolicyData>, 
    IAmStartedByMessages<StartInterestPeriod>,
    IHandleMessages<AccountBalanceRestored>,
    IHandleTimeouts<ChargeAccountInterest>
{
    private static ILog _logger = LogManager.GetLogger<AccountInterestPolicy>();
    
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AccountInterestPolicyData> mapper)
    {
        mapper.MapSaga(saga => saga.AccountId)
            .ToMessage<StartInterestPeriod>(msg => msg.AccountId)
            .ToMessage<AccountBalanceRestored>(msg => msg.AccountId);
    }

    public Task Handle(StartInterestPeriod message, IMessageHandlerContext context)
    {
        Data.AccountId = message.AccountId;
        Data.InterestPeriod = new Tuple<DateTime, DateTime>(message.StartDate, message.StartDate.AddMonths(3).AddDays(-1));
        Data.TotalInterest = 0;
        
        _logger.Info($"Interest period started for account [{Data.AccountId}] from {Data.InterestPeriod.Item1} to {Data.InterestPeriod.Item2}");
        
        return RequestTimeout(context, TimeSpan.FromSeconds(60), new ChargeAccountInterest
        {
            AccountId = Data.AccountId
        });
    }

    public Task Handle(AccountBalanceRestored message, IMessageHandlerContext context)
    {
        _logger.Info($"Balance restored for account [{message.AccountId}] - Calculate interest based on balance {message.LowestBalance} and days {message.DaysUnderZero}.");
        var interestPerDay = -message.LowestBalance * 0.06m;
        var totalInterest = interestPerDay * message.DaysUnderZero / 365;
        Data.TotalInterest+= totalInterest;
        
        _logger.Info($"Interest due {totalInterest}. Added to total due.");

        return Task.CompletedTask;
    }

    public Task Timeout(ChargeAccountInterest state, IMessageHandlerContext context)
    {
        // Charge total interest collected in Data.InterestCollected
        _logger.Info($"Charging {Data.TotalInterest} interest for account [{Data.AccountId}]");
       
        MarkAsComplete();
        return context.SendLocal(new StartInterestPeriod
        {
            AccountId = Data.AccountId,
            StartDate = Data.InterestPeriod.Item2.AddDays(1)
        });
    }
}