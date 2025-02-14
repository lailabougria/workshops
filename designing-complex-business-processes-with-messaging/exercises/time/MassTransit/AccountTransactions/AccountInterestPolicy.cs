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

    }
    public Event<NegativeAccountBalance> NegativeAccountBalanceDetected { get; private set; }
    public Event<CreditAmountTransferred> CreditAmountTransferred { get; private set; }
    public Event<DebitAmountTransferred> DebitAmountTransferred { get; private set; }
    
    public State NegativeBalance { get; private set; }
    public State Replenished { get; private set; }
    
    private void ConfigureCorrelationIds()
    {
        InstanceState(x => x.CurrentState);

        Event(() => NegativeAccountBalanceDetected, x => x.CorrelateById(context => context.Message.AccountId));
        Event(() => CreditAmountTransferred, x => x.CorrelateById(context => context.Message.AccountId));
        Event(() => DebitAmountTransferred, x => x.CorrelateById(context => context.Message.AccountId));

        Initially(
            When(NegativeAccountBalanceDetected)
                .Then(context =>
                {
                    context.Saga.AccountId = context.Message.AccountId;
                    context.Saga.Balance = context.Message.Balance;
                    context.Saga.NegativeAccountBalanceStartDate = context.Message.BalanceTimestamp;
                })
                .TransitionTo(NegativeBalance)
        );
    }
}