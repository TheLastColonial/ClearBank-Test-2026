namespace ClearBank.DeveloperTest.Exceptions;

using ClearBank.DeveloperTest.Types;
using System;

public class InvalidSchemeException : InvalidOperationException
{
    public PaymentScheme PaymentScheme { get; init; }
    public string AccountNumber { get; init; }

    public InvalidSchemeException(PaymentScheme paymentScheme, string accountNumber) : base("Scheme for payment is invalid for this account")
    {
        PaymentScheme = paymentScheme;
        AccountNumber = accountNumber;
    }
}