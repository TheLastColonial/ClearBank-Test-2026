namespace ClearBank.DeveloperTest.Tests.Exceptions;

using ClearBank.DeveloperTest.Exceptions;
using NodaMoney;
using System;

public class AccountExceptionTests
{
    private AccountingException _exception;
    private string _accountId;
    private Money _money;

    public AccountExceptionTests() 
    {
        _accountId = "12345";
        _money = Money.PoundSterling(1);
        _exception = new AccountingException(_accountId, _money);
    }

    [Fact]
    public void AssignableAndProperties()
    {
        Assert.IsAssignableFrom<InvalidOperationException>(_exception);
        Assert.Equal(_accountId, _exception.AccountNumber);
        Assert.Equal(_money, _exception.Delta);

        var message = $"Cannot update account {_accountId} with money provided";
        Assert.Contains(message, _exception.Message);
    }
}
