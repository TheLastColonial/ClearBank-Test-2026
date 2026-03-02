namespace ClearBank.DeveloperTest.Exceptions;

using NodaMoney;
using System;

public class AccountingException : InvalidOperationException
{
    public string AccountNumber { get; init; }
    public Money Delta { get; init; }

    public AccountingException(string accountNumber, Money delta) : this($"Cannot update account {accountNumber} with money provided", accountNumber, delta)
    {
    }

    public AccountingException(string message, string accountNumber, Money delta) : base(message)
    {
        AccountNumber = accountNumber;
        Delta = delta;
    }
}
