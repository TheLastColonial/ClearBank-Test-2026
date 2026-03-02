using ClearBank.DeveloperTest.Exceptions;
using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Tests.Exceptions;

public class InvalidSchemeExceptionTests
{
    private InvalidSchemeException _exception;
    private PaymentScheme _scheme = PaymentScheme.Chaps;
    private string _accountNumber = "12345";

    public InvalidSchemeExceptionTests()
    {
        _exception = new InvalidSchemeException(_scheme, _accountNumber);
    }

    [Fact]
    public void AssignAndProeprties()
    {
        Assert.IsAssignableFrom<InvalidOperationException>(_exception);
        Assert.Equal(_scheme, _exception.PaymentScheme);
        Assert.Equal(_accountNumber, _exception.AccountNumber);
        Assert.Equal("Scheme for payment is invalid for this account", _exception.Message);
    }
}
