using System;
using NServiceBus;

namespace AccountTransactions;

class AccountInterestPolicyData : ContainSagaData
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime NegativeAccountBalanceStartDate { get; set; }
}