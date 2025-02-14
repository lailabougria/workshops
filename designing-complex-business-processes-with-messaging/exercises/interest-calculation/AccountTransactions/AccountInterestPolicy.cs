using System;
using NServiceBus;
using NServiceBus.Logging;
using System.Threading.Tasks;
using AccountTransactions.Contracts;

namespace AccountTransactions;

class AccountInterestPolicy : Saga<AccountInterestPolicyData>, IAmStartedByMessages<StartInterestPeriod>
{
    static ILog _logger = LogManager.GetLogger<AccountInterestPolicy>();

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AccountInterestPolicyData> mapper)
    {
        // If you add messages that are handled by the saga, don't forget to map them here
        // This is not required for timeouts
        mapper.MapSaga(sagaData => sagaData.AccountId)
            .ToMessage<StartInterestPeriod>(message => message.AccountId);
    }

    public Task Handle(StartInterestPeriod message, IMessageHandlerContext context)
    {
        Data.AccountId = message.AccountId;
        
        _logger.Info($"Interest period started for account [{Data.AccountId}]");

        return Task.CompletedTask;
    }
}