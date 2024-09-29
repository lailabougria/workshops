using AccountTransactions.Contracts;
using NServiceBus.Testing;
using NUnit.Framework;

namespace AccountTransactions.Tests;

[TestFixture]
public class AccountBalancePolicyTests
{
    [Test]
    public async Task WhenNegativeAccountBalanceOccurs_SagaIsStartedAndReminderIsSet()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        
        // Process NegativeAccountBalance and make assertions on the result
        var negativeBalanceHandled = await testableSaga.Handle(negativeBalanceDetected);
        
        // Assertions
        Assert.That(negativeBalanceHandled, Is.Not.Null);
        Assert.That(negativeBalanceHandled.Completed, Is.False);
            
        var negativeAccountBalanceReminder = negativeBalanceHandled.FindTimeoutMessage<NegativeAccountBalanceReminder>();
        Assert.That(negativeAccountBalanceReminder, Is.Not.Null);
        Assert.That(negativeAccountBalanceReminder.NumberOfTimesReminded, Is.EqualTo(0));
    }
    
    [Test]
    public async Task WhenNegativeAccountBalanceOccurs_CanHandleDebitTransaction()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        await testableSaga.Handle(negativeBalanceDetected);
        
        // Process debit transaction
        var debitTransaction = new DebitAmountTransferred { AccountId = accountId, Amount = 200 };
        var debitTransactionHandled = await testableSaga.Handle(debitTransaction);
        
        // Assertions
        Assert.That(debitTransactionHandled, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(debitTransactionHandled.Completed, Is.False);
            Assert.That(debitTransactionHandled.SagaDataSnapshot.Balance, Is.EqualTo(-700));
        });
    }
    
    [Test]
    public async Task WhenNegativeAccountBalanceOccurs_CanHandleCreditTransaction()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        await testableSaga.Handle(negativeBalanceDetected);
        
        // Process credit transaction
        var creditAmountTransferred = new CreditAmountTransferred() { AccountId = accountId, Amount = 200 };
        var creditTransactionHandled = await testableSaga.Handle(creditAmountTransferred);
        
        // Assertions
        Assert.That(creditTransactionHandled, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(creditTransactionHandled.Completed, Is.False);
            Assert.That(creditTransactionHandled.SagaDataSnapshot.Balance, Is.EqualTo(-300));
        });
    }
    
    [Test]
    public async Task WhenFirstReminderElapses_SetsANewReminder()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        
        await testableSaga.Handle(negativeBalanceDetected);
        var messagesHandled = await testableSaga.AdvanceTime(TimeSpan.FromSeconds(1.5));
        
        Assert.That(messagesHandled, Has.Length.EqualTo(1));
        var timeoutElapsed = messagesHandled.First();
        Assert.That(timeoutElapsed, Is.Not.Null);
        var nextReminder = timeoutElapsed.FindTimeoutMessage<NegativeAccountBalanceReminder>();
        Assert.That(nextReminder, Is.Not.Null);
        Assert.That(nextReminder.NumberOfTimesReminded, Is.EqualTo(1));
    }
    
    [Test]
    public async Task WhenSecondReminderElapses_SetsANewReminder()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        
        await testableSaga.Handle(negativeBalanceDetected);
        // lapse to past the first reminder
        await testableSaga.AdvanceTime(TimeSpan.FromSeconds(1.1));
        var secondLapse = await testableSaga.AdvanceTime(TimeSpan.FromSeconds(2.1));
        
        var timeoutElapsed = secondLapse.FirstOrDefault();
        Assert.That(timeoutElapsed, Is.Not.Null);
        var nextReminder = timeoutElapsed.FindTimeoutMessage<NegativeAccountBalanceReminder>();
        Assert.That(nextReminder, Is.Not.Null);
        Assert.That(nextReminder.NumberOfTimesReminded, Is.EqualTo(2));
    }
    
    [Test]
    public async Task WhenThirdReminderElapses_SchedulesForTheAccountToBeBlocked()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        
        await testableSaga.Handle(negativeBalanceDetected);
        // lapse to past the first reminder
        await testableSaga.AdvanceTime(TimeSpan.FromSeconds(1.1));
        // lapse to past the second reminder
        await testableSaga.AdvanceTime(TimeSpan.FromSeconds(2.1));
        // jump to past the third reminder
        var thirdLapse = await testableSaga.AdvanceTime(TimeSpan.FromSeconds(3.1));
        
        var timeoutElapsed = thirdLapse.FirstOrDefault();
        Assert.That(timeoutElapsed, Is.Not.Null);
        var blockAccountMessage = timeoutElapsed.FindSentMessage<BlockAccount>();
        Assert.That(blockAccountMessage, Is.Not.Null);
        Assert.That(blockAccountMessage.AccountId, Is.EqualTo(accountId));
    }
    
    [Test]
    public async Task WhenBalanceReplenishes_CompletesTheSaga()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountBalancePolicy, AccountBalancePolicyData>();
        
        // Set up a started saga
        var accountId = Guid.NewGuid();
        var negativeBalanceDetected = new NegativeAccountBalance
        {
            AccountId = accountId,
            Balance = -500,
            BalanceTimestamp = DateTime.UtcNow
        };
        
        await testableSaga.Handle(negativeBalanceDetected);
        var debitTransaction = new DebitAmountTransferred { AccountId = accountId, Amount = 200 };
        var creditTransaction1 = new CreditAmountTransferred { AccountId = accountId, Amount = 200 };
        var creditTransaction2 = new CreditAmountTransferred { AccountId = accountId, Amount = 300 };
        var debitTransaction2 = new DebitAmountTransferred { AccountId = accountId, Amount = 100 };
        var creditTransaction3 = new CreditAmountTransferred { AccountId = accountId, Amount = 350 };
        
        await testableSaga.Handle(debitTransaction);
        await testableSaga.Handle(creditTransaction1);
        await testableSaga.Handle(creditTransaction2);
        await testableSaga.Handle(debitTransaction2);
        var replenishingTransaction = await testableSaga.Handle(creditTransaction3);

        Assert.That(replenishingTransaction.Completed, Is.True);
        Assert.That(replenishingTransaction.SagaDataSnapshot.Balance, Is.EqualTo(50));
    }
}