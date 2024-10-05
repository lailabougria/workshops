using System.Threading.Tasks;
using AccountTransactions.Contracts;
using NServiceBus;
using NServiceBus.Logging;

namespace AccountTransactions;

internal class AccountBlockedHandler : IHandleMessages<AccountBlocked>
{
    static ILog _logger = LogManager.GetLogger<AccountBlockedHandler>();
    
    public Task Handle(AccountBlocked message, IMessageHandlerContext context)
    {
        _logger.Warn("Account is now blocked.");
        return Task.CompletedTask;
    }
}