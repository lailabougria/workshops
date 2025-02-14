using System;
using NServiceBus;

namespace AccountTransactions;

public class AccountBalancePolicyData : ContainSagaData
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime NegativeAccountBalanceStartDate { get; set; }
    public decimal LowestBalance { get; set; }
}