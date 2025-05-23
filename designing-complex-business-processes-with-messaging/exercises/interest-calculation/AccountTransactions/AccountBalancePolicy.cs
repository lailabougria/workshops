using System;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using AccountTransactions.Contracts;

namespace AccountTransactions;

public class AccountBalancePolicy : Saga<AccountBalancePolicyData>, 
    IAmStartedByMessages<NegativeAccountBalance>,
    IHandleMessages<CreditAmountTransferred>,
    IHandleMessages<DebitAmountTransferred>,
    IHandleTimeouts<NegativeAccountBalanceReminder>,
    IHandleTimeouts<BlockAccountReminder>
{
    static ILog _logger = LogManager.GetLogger<AccountBalancePolicy>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AccountBalancePolicyData> mapper)
    {
        // If you add messages that are handled by the saga, don't forget to map them here
        // This is not required for timeouts
        mapper.MapSaga(sagaData => sagaData.AccountId)
            .ToMessage<NegativeAccountBalance>(message => message.AccountId)
            .ToMessage<CreditAmountTransferred>(message => message.AccountId)
            .ToMessage<DebitAmountTransferred>(message => message.AccountId);
    }

    public Task Handle(NegativeAccountBalance message, IMessageHandlerContext context)
    {
        Data.AccountId = message.AccountId;
        Data.Balance = message.Balance;
        Data.LowestBalance = message.Balance;
        Data.NegativeAccountBalanceStartDate = message.BalanceTimestamp;
        
        _logger.Info($"Negative balance of {Data.Balance} detected for account [{Data.AccountId}] - Start tracking.");
        
        // Schedule first reminder
        return RequestTimeout(context, TimeSpan.FromSeconds(1), new NegativeAccountBalanceReminder
        {
            NumberOfTimesReminded = 0
        });
    }

    public async Task Handle(CreditAmountTransferred message, IMessageHandlerContext context)
    {
        Data.Balance += message.Amount;
        _logger.Info($"Balance was credited with {message.Amount}. New balance is {Data.Balance}");
        
        if (Data.Balance > 0)
        {
            _logger.Warn($"Balance is now replenished. Stop tracking.");
            await context.Publish<AccountBalanceRestored>(restored =>
            {
                restored.AccountId = Data.AccountId; 
                restored.LowestBalance = Data.LowestBalance; 
                restored.DaysUnderZero = Convert.ToInt32(DateTime.UtcNow.Subtract(Data.NegativeAccountBalanceStartDate).TotalMilliseconds/100); 
            });
            MarkAsComplete();
        }
    }

    public Task Handle(DebitAmountTransferred message, IMessageHandlerContext context)
    {
        Data.Balance -= message.Amount;
        _logger.Info($"Balance was debited with {message.Amount}. New balance is {Data.Balance}");
        
        if (Data.LowestBalance > Data.Balance)
        {
            Data.LowestBalance = Data.Balance;
        }
        return Task.CompletedTask;
    }

    public async Task Timeout(NegativeAccountBalanceReminder state, IMessageHandlerContext context)
    {
        _logger.Info($"Timeout received, balance is still negative: {Data.Balance}");
        
        state.NumberOfTimesReminded++;
        _logger.Warn($"Account [{Data.AccountId}] has a negative balance of {Data.Balance}. Sending reminder {state.NumberOfTimesReminded}.");
        
        if (state.NumberOfTimesReminded < 3)
        {
            // Schedule next reminder
            await RequestTimeout(context, TimeSpan.FromSeconds(1), new NegativeAccountBalanceReminder
            {
                NumberOfTimesReminded = state.NumberOfTimesReminded
            });
        }
        else
        {
            // We are done with the reminders, now we'll schedule a timeout to block the account
            await RequestTimeout(context, TimeSpan.FromSeconds(1), new BlockAccountReminder());
        }
    }

    public async Task Timeout(BlockAccountReminder state, IMessageHandlerContext context)
    {
        // Send a message to block the account
        // Remember that this is a saga and therefore should only orchestrate, not execute
        // the business logic itself
        await context.Send(new BlockAccount
        {
            AccountId = Data.AccountId
        });
        MarkAsComplete();
    }
}

public class NegativeAccountBalanceReminder
{
    public int NumberOfTimesReminded { get; set; }
}

public class BlockAccountReminder
{
}