using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

public class AccountBlocked : IEvent
{
    public Guid AccountId { get; set; }
}