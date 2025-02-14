using System;

namespace AccountTransactions.Contracts;

// This event is published every time an amount is credited (+) to an account
public class CreditAmountTransferred
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
}