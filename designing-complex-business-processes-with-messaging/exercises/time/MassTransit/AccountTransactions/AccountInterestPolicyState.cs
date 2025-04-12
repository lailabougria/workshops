using System;
using MassTransit;

namespace AccountTransactions;

class AccountInterestPolicyState : SagaStateMachineInstance
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime NegativeAccountBalanceStartDate { get; set; }
    
    public Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public decimal LowestBalance { get; set; }
}