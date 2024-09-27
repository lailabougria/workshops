using System;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using AccountTransactions.Contracts;

namespace AccountTransactions;

class AccountInterestPolicy : Saga<AccountInterestPolicyData>, IAmStartedByMessages<NegativeAccountBalance>
{
    static ILog _logger = LogManager.GetLogger<AccountInterestPolicy>();
    Random _random = new Random();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AccountInterestPolicyData> mapper)
    {
        // If you add messages that are handled by the saga, don't forget to map them here
        // This is not required for timeouts
        mapper.MapSaga(sagaData => sagaData.AccountId)
            .ToMessage<NegativeAccountBalance>(message => message.AccountId);
    }

    public Task Handle(NegativeAccountBalance message, IMessageHandlerContext context)
    {
        Data.AccountId = message.AccountId;
        Data.Balance = message.Balance;
        
        _logger.Info($"Negative balance detected for account [{Data.AccountId}] - Start tracking.");
        return Task.CompletedTask;
    }
}