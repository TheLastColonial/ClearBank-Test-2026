namespace ClearBank.DeveloperTest.Tests.Types;

using System.Collections.Generic;
using ClearBank.DeveloperTest.Exceptions;
using ClearBank.DeveloperTest.Types;
using NodaMoney;

public class AccountTests
{
    private Account _account;

    public static IEnumerable<object[]> CreditData =>
        new List<object[]>
        {
            new object[] { new Money(20, "GBP"), new Money(20, "GBP"), 40.0m },
            new object[] { new Money(999, "GBP"), new Money(1, "GBP"), 1000.0m }
        };

    [Theory]
    [MemberData(nameof(CreditData))]
    public void GivenSameCurrency_WhenCredit_ThenUpdateBalance(Money balance, Money credit, decimal total)
    {
        // Given
        _account = new Account("12345", balance, AccountStatus.Live, AllowedPaymentSchemes.Bacs);

        // When
        _account.Credit(credit);

        // Then
        Assert.Equal(total, _account.Balance.Amount);
    }

    public static IEnumerable<object[]> DebitData =>
        new List<object[]>
        {
            new object[] { new Money(20, "GBP"), new Money(20, "GBP"), 0.0m },
            new object[] { new Money(999, "GBP"), new Money(1, "GBP"), 998.0m }
        };

    [Theory]
    [MemberData(nameof(DebitData))]
    public void GivenSameCurrency_WhenDebit_ThenUpdateBalance(Money balance, Money credit, decimal total)
    {
        // Given
        _account = GetValidAccount(balance);

        // When
        _account.Debit(credit);

        // Then
        Assert.Equal(total, _account.Balance.Amount);
    }

    public static IEnumerable<object[]> DiffCurrency =>
        new List<object[]>
        {
            new object[] { new Money(20, "GBP"), new Money(20, "USD") },
            new object[] { new Money(999, "GBP"), new Money(1, "EUR") }
        };

    [Theory]
    [MemberData(nameof(DiffCurrency))]
    public void GivenDifferentCurrency_WhenCredit_ThenThrow(Money balance, Money credit)
    {
        // Given
        _account = new Account("12345", balance, AccountStatus.Live, AllowedPaymentSchemes.Bacs);

        // When
        var ex = Assert.Throws<InvalidCurrencyException>(() => _account.Credit(credit));

        // Then
    }

    [Theory]
    [MemberData(nameof(DiffCurrency))]
    public void GivenDifferentCurrency_WhenDebit_ThenThrow(Money balance, Money credit)
    {
        // Given
        _account = new Account("12345", balance, AccountStatus.Live, AllowedPaymentSchemes.Bacs);

        // When
        var ex = Assert.Throws<InvalidCurrencyException>(() => _account.Debit(credit));

        // Then
    }

    [Theory]
    [InlineData(AccountStatus.Disabled)]
    public void GivenAccountInvalidState_WhenCredit_ThenThrow(AccountStatus status)
    {
        // Given
        _account = new Account("12345", Money.PoundSterling(20), status, AllowedPaymentSchemes.Bacs);

        // When
        var ex = Assert.Throws<InvalidAccountStateException>(() => _account.Credit(Money.PoundSterling(20)));

        // Then
        Assert.Equal("Cannot credit account due to account status", ex.Message);
        Assert.Equal(_account.AccountNumber, ex.AccountNumber);
    }

    [Theory]
    [InlineData(AccountStatus.InboundPaymentsOnly)]
    [InlineData(AccountStatus.Disabled)]
    public void GivenAccountInvalidState_WhenDebit_ThenThrow(AccountStatus status)
    {
        // Given
        _account = new Account("12345", Money.PoundSterling(20), status, AllowedPaymentSchemes.Bacs);

        // When
        var ex = Assert.Throws<InvalidAccountStateException>(() => _account.Debit(Money.PoundSterling(20)));

        // Then
        Assert.Equal("Cannot debit account due to account status", ex.Message);
        Assert.Equal(_account.AccountNumber, ex.AccountNumber);
    }


    private Account GetValidAccount(Money start) =>
        new Account("12345", start, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
}
