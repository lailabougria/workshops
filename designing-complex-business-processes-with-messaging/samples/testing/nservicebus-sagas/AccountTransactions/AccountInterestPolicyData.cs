using System;
using NServiceBus;

namespace AccountTransactions;

public class AccountInterestPolicyData : ContainSagaData
{
    public Guid AccountId { get; set; }
    public Tuple<DateTime, DateTime> InterestPeriod { get; set; }
    public decimal TotalInterest { get; set; }
}