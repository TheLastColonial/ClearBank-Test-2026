namespace ClearBank.DeveloperTest.Exceptions;

using System;

public class InvalidAccountStateException : InvalidOperationException
{
    public string AccountNumber { get; init; }

    public InvalidAccountStateException(string accountNumber) : this($"Account ({accountNumber}) was not in valid state to complete operaton", accountNumber)
    {
    }

    public InvalidAccountStateException(string message, string accountNumber) : base(message)
    {
        AccountNumber = accountNumber;
    }
}
