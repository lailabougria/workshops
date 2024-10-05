using System;
using NServiceBus;

namespace AccountTransactions.Contracts;

public class BlockAccount : ICommand
{
    public Guid AccountId { get; set; }
}