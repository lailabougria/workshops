using AccountTransactions.Contracts;
using NServiceBus.Testing;
using NUnit.Framework;

namespace AccountTransactions.Tests;

[TestFixture]
public class AccountInterestPolicyTests
{
    [Test]
    public async Task WhenInterestPeriodStarts_SagaIsStartedAndReminderIsSet()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountInterestPolicy, AccountInterestPolicyData>();
        var accountId = Guid.NewGuid();
        var startInterestPeriod = new StartInterestPeriod()
        {
            AccountId = accountId, 
            StartDate = DateTime.Today
        };
        
        // Process StartInterestPeriod message and make assertions on the result
        var interestPeriodStartHandled = await testableSaga.Handle(startInterestPeriod);
        
        // Assertions
        Assert.That(interestPeriodStartHandled, Is.Not.Null);
        Assert.That(interestPeriodStartHandled.Completed, Is.False);
        Assert.That(interestPeriodStartHandled.SagaDataSnapshot.InterestPeriod.Item1, Is.EqualTo(DateTime.Today));
        Assert.That(interestPeriodStartHandled.SagaDataSnapshot.InterestPeriod.Item2, Is.EqualTo(DateTime.Today.AddMonths(3).AddDays(-1)));
        Assert.That(interestPeriodStartHandled.SagaDataSnapshot.AccountId, Is.EqualTo(accountId));
        Assert.That(interestPeriodStartHandled.SagaDataSnapshot.TotalInterest, Is.EqualTo(0));
       
        var chargeInterestReminder = interestPeriodStartHandled.FindTimeoutMessage<ChargeAccountInterest>();
        Assert.That(chargeInterestReminder, Is.Not.Null);
    }

    [Test]
    public async Task WhenMultipleDebtPeriodsExist_InterestIsAccumulated()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountInterestPolicy, AccountInterestPolicyData>();
        var accountId = Guid.NewGuid();
        var startInterestPeriod = new StartInterestPeriod()
        {
            AccountId = accountId, 
            StartDate = DateTime.Today
        };
        
        await testableSaga.Handle(startInterestPeriod);
        
        // Process multiple debt periods
        var debtPeriod1 = new AccountBalanceRestored { AccountId = accountId, LowestBalance = -500, DaysUnderZero = 3};
        var debtPeriod2 = new AccountBalanceRestored { AccountId = accountId, LowestBalance = -1250, DaysUnderZero = 7};
        var firstDebtPeriodHandled = await testableSaga.Handle(debtPeriod1);
        var secondDebtPeriodHandled = await testableSaga.Handle(debtPeriod2);
        
        // Assertions
        Assert.That(firstDebtPeriodHandled, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(firstDebtPeriodHandled.Completed, Is.False);
            Assert.That(firstDebtPeriodHandled.SagaDataSnapshot.TotalInterest, Is.GreaterThan(0));
        });
        Assert.That(secondDebtPeriodHandled, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(secondDebtPeriodHandled.Completed, Is.False);
            Assert.That(secondDebtPeriodHandled.SagaDataSnapshot.TotalInterest, Is.GreaterThan(firstDebtPeriodHandled.SagaDataSnapshot.TotalInterest));
        });
    }
    
    [Test]
    public async Task WhenInterestPeriodCompletes_NewPeriodIsStarted()
    {
        // Create the testable saga
        var testableSaga = new TestableSaga<AccountInterestPolicy, AccountInterestPolicyData>();
        var accountId = Guid.NewGuid();
        var startInterestPeriod = new StartInterestPeriod()
        {
            AccountId = accountId, 
            StartDate = DateTime.Today
        };
        
        await testableSaga.Handle(startInterestPeriod);
        
        // Process multiple debt periods
        var debtPeriod1 = new AccountBalanceRestored { AccountId = accountId, LowestBalance = -500, DaysUnderZero = 3};
        var debtPeriod2 = new AccountBalanceRestored { AccountId = accountId, LowestBalance = -1250, DaysUnderZero = 7};
        await testableSaga.Handle(debtPeriod1);
        var secondDebtPeriodHandled = await testableSaga.Handle(debtPeriod2);
        var fastForward = await testableSaga.AdvanceTime(TimeSpan.FromSeconds(60));

        var advanceTimeToPeriodFinish = fastForward.FirstOrDefault();
        Assert.That(advanceTimeToPeriodFinish, Is.Not.Null);
        Assert.Multiple((() =>
        {
            Assert.That(advanceTimeToPeriodFinish.Completed, Is.True);
            Assert.That(advanceTimeToPeriodFinish.SagaDataSnapshot.TotalInterest,
                Is.EqualTo(secondDebtPeriodHandled.SagaDataSnapshot.TotalInterest));
        }));
        
        var newPeriod = advanceTimeToPeriodFinish.FindSentMessage<StartInterestPeriod>();
        Assert.That(newPeriod, Is.Not.Null);
        Assert.Multiple((() =>
        {
            Assert.That(newPeriod.AccountId, Is.EqualTo(accountId));
            var expectedStartDate = advanceTimeToPeriodFinish.SagaDataSnapshot.InterestPeriod.Item2;
            expectedStartDate = expectedStartDate.AddDays(1);
            Assert.That(newPeriod.StartDate, Is.EqualTo(expectedStartDate));
            Assert.That(advanceTimeToPeriodFinish.SagaDataSnapshot.TotalInterest,
                Is.EqualTo(secondDebtPeriodHandled.SagaDataSnapshot.TotalInterest));
        }));
    }
}