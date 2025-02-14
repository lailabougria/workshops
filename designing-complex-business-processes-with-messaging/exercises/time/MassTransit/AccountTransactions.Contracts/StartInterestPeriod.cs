using System;

namespace AccountTransactions.Contracts;

public class StartInterestPeriod
{
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
}