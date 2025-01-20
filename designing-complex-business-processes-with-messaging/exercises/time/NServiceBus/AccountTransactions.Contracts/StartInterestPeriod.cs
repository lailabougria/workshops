using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

public class StartInterestPeriod : ICommand
{
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
}