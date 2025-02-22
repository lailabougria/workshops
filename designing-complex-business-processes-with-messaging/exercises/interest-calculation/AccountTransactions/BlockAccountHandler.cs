using System.Threading.Tasks;
using AccountTransactions.Contracts;
using NServiceBus;
using NServiceBus.Logging;

namespace AccountTransactions;

internal class BlockAccountHandler : IHandleMessages<BlockAccount>
{
    static ILog _logger = LogManager.GetLogger<BlockAccountHandler>();
    
    public Task Handle(BlockAccount message, IMessageHandlerContext context)
    {
        _logger.Warn("Blocking account.");
        return context.Publish<AccountBlocked>(blocked =>
        {
            blocked.AccountId = message.AccountId;
        });
    }
}