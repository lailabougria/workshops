using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

public class ChargeAccountInterest : ICommand
{
    public Guid AccountId { get; set; }
}