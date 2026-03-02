using ClearBank.DeveloperTest.Exceptions;

namespace ClearBank.DeveloperTest.Tests.Exceptions;

public class InvalidAccountStateExceptionTests
{
    private InvalidAccountStateException _exception;
    private string _accountNumber = "123456";

    public InvalidAccountStateExceptionTests()
    {
        _exception = new InvalidAccountStateException(_accountNumber);
    }

    [Fact]
    public void AssignableAndProperties()
    {
        Assert.IsAssignableFrom<InvalidOperationException>(_exception);
        Assert.Equal(_accountNumber, _exception.AccountNumber);

        var message = $"Account ({_accountNumber}) was not in valid state to complete operaton";
        Assert.Equal(message, _exception.Message);
    }
}
