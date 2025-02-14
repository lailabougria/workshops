using System;

namespace AccountTransactions.Contracts;

// This event is published every time an amount is debited (-) to an account
public class DebitAmountTransferred
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}