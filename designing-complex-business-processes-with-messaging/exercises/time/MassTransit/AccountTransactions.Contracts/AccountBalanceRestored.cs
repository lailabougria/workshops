using System;

namespace AccountTransactions.Contracts;

public class AccountBalanceRestored
{
    public Guid AccountId { get; set; }
    public decimal LowestBalance { get; set; }
    public int DaysUnderZero { get; set; }
}