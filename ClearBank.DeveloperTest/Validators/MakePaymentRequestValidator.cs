namespace ClearBank.DeveloperTest.Validators;

using ClearBank.DeveloperTest.Types;
using FluentValidation;

public interface IMakePaymentRequestValidator : IValidator<MakePaymentRequest>
{
}

/// <summary>
/// Validate request model for making payments
/// </summary>
public class MakePaymentRequestValidator : AbstractValidator<MakePaymentRequest>, IMakePaymentRequestValidator
{
    public MakePaymentRequestValidator()
    {
        RuleFor(request => request.CreditorAccountNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("Creditor Account Number must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.CreditorAccountNumber)
                    .MaximumLength(255)
                    .WithMessage("Creditor Account Number must be less than 225");
            });

        RuleFor(request => request.DebtorAccountNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("Debtor Account Number must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.DebtorAccountNumber)
                    .MaximumLength(255)
                    .WithMessage("Debtor Account Number must be less than 225");
            });

        RuleFor(request => request.Amount)
            .NotNull()
            .WithMessage("Amount must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.Amount)
                    .GreaterThan(0)
                    .WithMessage("Amount must be positive value");
            });

        RuleFor(request => request.PaymentDate)
            .NotNull()
            .WithMessage("Payment Date must be provided");
        // todo greater than today - what about deferred payments ?

        RuleFor(request => request.PaymentScheme)
            .NotNull()
            .WithMessage("Payment Scheme must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.PaymentScheme)
                    .Must(ValidPaymentScheme)
                    .WithMessage("Payment Scheme is not valid");
            });
    }

    private bool ValidPaymentScheme(PaymentScheme scheme) =>
        scheme != PaymentScheme.Unknown;
}
