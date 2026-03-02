namespace ClearBank.DeveloperTest.Exceptions;

using System;

public class AccountNotFoundException : Exception
{
    public string AccountNumber;

    public AccountNotFoundException(string accountNumber) : base("Account could not be found")
    {
        AccountNumber = accountNumber;
    }
}