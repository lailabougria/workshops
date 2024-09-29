using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

public class AccountBalanceRestored: IEvent
{
    public Guid AccountId { get; set; }
    public decimal LowestBalance { get; set; }
    public int DaysUnderZero { get; set; }
}