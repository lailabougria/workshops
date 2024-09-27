using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

// This event is published every time an amount is debited (-) to an account
public class DebitAmountTransferred : IEvent
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}