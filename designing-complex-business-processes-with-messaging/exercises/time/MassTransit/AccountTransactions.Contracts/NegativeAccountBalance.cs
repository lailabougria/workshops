using System;

namespace AccountTransactions.Contracts;

// This event is published every time an account balance goes negative.
// It is not republished for subsequent transactions that further reduce the balance.
public class NegativeAccountBalance
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime BalanceTimestamp { get; set; }
}