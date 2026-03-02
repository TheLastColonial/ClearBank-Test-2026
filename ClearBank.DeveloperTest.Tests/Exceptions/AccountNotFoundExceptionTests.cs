using ClearBank.DeveloperTest.Exceptions;

namespace ClearBank.DeveloperTest.Tests.Exceptions;

public class AccountNotFoundExceptionTests
{
    private AccountNotFoundException _exception;
    private string _accountNumber = "12345";

    public AccountNotFoundExceptionTests()
    {
        _exception = new AccountNotFoundException(_accountNumber);
    }

    [Fact]
    public void AssignAndProperties()
    {
        Assert.IsAssignableFrom<Exception>(_exception);
        Assert.Equal(_accountNumber, _exception.AccountNumber);
        Assert.Equal("Account could not be found", _exception.Message);
    }
}
