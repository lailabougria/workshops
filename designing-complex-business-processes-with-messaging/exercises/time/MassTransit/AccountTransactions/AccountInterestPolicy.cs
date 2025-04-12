using System;
using System.Threading.Tasks;
using AccountTransactions.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AccountTransactions;

class AccountInterestPolicy : MassTransitStateMachine<AccountInterestPolicyState>
{
    private readonly ILogger<AccountInterestPolicy> _logger;
    private Random _random = new Random();

    public AccountInterestPolicy(ILogger<AccountInterestPolicy> logger)
    {
        _logger = logger;
        InstanceState(x => x.CurrentState);
        ConfigureCorrelationIds();
        
        Initially(
            When(NegativeAccountBalanceDetected)
                .Then(context =>
                {
                    context.Saga.AccountId = context.Message.AccountId;
                    context.Saga.Balance = context.Message.Balance;
                    context.Saga.NegativeAccountBalanceStartDate = context.Message.BalanceTimestamp;
                    _logger.LogInformation($"Negative balance of {context.Saga.Balance} detected for account [{context.Saga.AccountId}] - Start tracking.");
                })
                .Send(context => context.Init<ReminderMessage>(new
                {
                    AccountId = context.Message.AccountId,
                    NumberOfTimesReminded = 0
                }), x => x.Delay = TimeSpan.FromSeconds(1))
                .TransitionTo(NegativeBalance)
            );

        During(NegativeBalance,
            When(DebitAmountTransferred)
                .Then(context =>
                {
                    context.Saga.Balance -= context.Message.Amount;
                    _logger.LogInformation($"Balance was debited with {context.Message.Amount}. New balance is {context.Saga.Balance}");
                    
                    if (context.Saga.LowestBalance > context.Saga.Balance)
                    {
                        context.Saga.LowestBalance = context.Saga.Balance;
                    }
                }).TransitionTo(NegativeBalance),
                When(CreditAmountTransferred)
                    .Then(context =>
                    {
                        context.Saga.Balance += context.Message.Amount;
                        _logger.LogInformation($"Balance was credited with {context.Message.Amount}. New balance is {context.Saga.Balance}");

                        if (context.Saga.Balance > 0)
                        {
                            _logger.LogWarning("Balance is now replenished. Stop tracking.");

                            context.Publish(new AccountBalanceRestored
                            {
                                AccountId = context.Saga.AccountId,
                                LowestBalance = context.Saga.LowestBalance,
                                DaysUnderZero = Convert.ToInt32(DateTime.UtcNow.Subtract(context.Saga.NegativeAccountBalanceStartDate).TotalMilliseconds/100)
                            });
                        }
                    })
                    .If(context => context.Saga.Balance > 0, x => x.TransitionTo(Replenished)),
                When(ReminderReceived)
                    .Then(context =>
                    {
                        int reminderCount = context.Message.NumberOfTimesReminded;
                        int newReminderCount = reminderCount + 1;
                        _logger.LogWarning($"Received reminder {reminderCount} for account [{context.Saga.AccountId}] with negative balance {context.Saga.Balance}");
                        
                        if (reminderCount < 3)
                        {
                            // Send another reminder
                            _logger.LogInformation($"Sending reminder {newReminderCount+1} for account [{context.Saga.AccountId}]");
                            context.ScheduleSend<ReminderMessage>(DateTime.UtcNow.AddSeconds(newReminderCount), new
                            {
                                AccountId = context.Saga.AccountId,
                                NumberOfTimesReminded = newReminderCount
                            });
                        }
                        else
                        {
                            // Block the account after the third reminder
                            _logger.LogWarning($"Account [{context.Saga.AccountId}] has been in negative balance for too long. Blocking account.");
                            context.Publish(new BlockAccount
                            {
                                AccountId = context.Saga.AccountId,
                                Reason = $"Account has been in negative balance (${context.Saga.Balance}) for too long"
                            });
                        }
                    })
            );
    }
    public Event<NegativeAccountBalance> NegativeAccountBalanceDetected { get; private set; }
    public Event<CreditAmountTransferred> CreditAmountTransferred { get; private set; }
    public Event<DebitAmountTransferred> DebitAmountTransferred { get; private set; }
    public Event<ReminderMessage> ReminderReceived { get; private set; }
    
    public State NegativeBalance { get; private set; }
    public State Replenished { get; private set; }
    
    private void ConfigureCorrelationIds()
    {
        InstanceState(x => x.CurrentState);

        Event(() => NegativeAccountBalanceDetected, x => x.CorrelateById(context => context.Message.AccountId));
        Event(() => CreditAmountTransferred, x => x.CorrelateById(context => context.Message.AccountId));
        Event(() => DebitAmountTransferred, x => x.CorrelateById(context => context.Message.AccountId));
        Event(() => ReminderReceived, x => x.CorrelateById(context => context.Message.AccountId));
    }

    public class ReminderMessage
    {
        public Guid AccountId { get; set; }
        public int NumberOfTimesReminded { get; set; }
    }
}
