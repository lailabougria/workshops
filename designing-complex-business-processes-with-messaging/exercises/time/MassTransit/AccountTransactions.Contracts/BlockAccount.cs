using System;

namespace AccountTransactions.Contracts
{
    public class BlockAccount
    {
        public Guid AccountId { get; set; }
        public string Reason { get; set; }
    }
}
