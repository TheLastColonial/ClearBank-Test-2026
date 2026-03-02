using System;
using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validators;

/// <summary>
/// Model to support validation of <see cref="MakePaymentRequest"/> against a specific <see cref="Account"/>
/// </summary>
/// <param name="Request">User request</param>
/// <param name="Account">Debtors Account</param>
public record PaymentSchemeValidationModel(MakePaymentRequest Request, Account Account)
{
}

public interface IPaymentSchemesValidator : IValidator<PaymentSchemeValidationModel>
{
}

/// <summary>
/// Validate buiness logic of if a payment is valid for a given payement rail
/// </summary>
/// <remarks><see cref="Account"/> must be provided as context</remarks>
public class PaymentSchemeValidator : AbstractValidator<PaymentSchemeValidationModel>, IPaymentSchemesValidator
{
    private readonly IReadOnlyDictionary<PaymentScheme, ISchemeValidator> _validators;

    public PaymentSchemeValidator(IEnumerable<ISchemeValidator> valdiators)
    {
        _validators = valdiators.ToDictionary(validator => validator.Scheme);

        RuleFor(model => model.Request.PaymentScheme)
            .NotNull()
            .Must(scheme => scheme != PaymentScheme.Unknown)
            .WithMessage("Supported Payment Scheme not provided");

        RuleFor(model => model.Account)
            .NotNull()
            .WithMessage("Debtor Account required to validate payment");

        When(model =>
            model.Request.PaymentScheme is not PaymentScheme.Unknown
            && model.Account != null,
            () =>
            {
                RuleFor(model => model)
                    .Custom((model, context) =>
                    {
                        if (!_validators.TryGetValue(model.Request.PaymentScheme, out var validator))
                        {
                            throw new ArgumentOutOfRangeException(nameof(model.Request.PaymentScheme), "Payment Scheme does not have matching validator");
                        }

                        var result = validator.Validate(context);
                    });
            });
    }
}

public interface ISchemeValidator : IValidator<PaymentSchemeValidationModel>
{
    PaymentScheme Scheme { get; }
}

public class BacsValidator : AbstractValidator<PaymentSchemeValidationModel>, ISchemeValidator
{
    public PaymentScheme Scheme => PaymentScheme.Bacs;

    public BacsValidator()
    {
        RuleFor(model => model.Request.PaymentScheme)
            .Must((model, scheme) => model.Account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
            .WithMessage("BACS not enabled for this account");
    }
}

public class FasterPaymentsValidator : AbstractValidator<PaymentSchemeValidationModel>, ISchemeValidator
{
    public PaymentScheme Scheme => PaymentScheme.FasterPayments;

    public FasterPaymentsValidator()
    {
        RuleFor(model => model.Request.PaymentScheme)
            .Must((model, scheme) => model.Account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
            .WithMessage("Faster Payments not enabled for this account");

        RuleFor(model => model.Request.Amount)
            .Must((model, amount) => model.Account.Balance.Amount >= model.Request.Amount)
            .WithMessage("Balance on account is less than requested amount for Faster payments");
    }
}

public class ChapsValidator : AbstractValidator<PaymentSchemeValidationModel>, ISchemeValidator
{
    public PaymentScheme Scheme => PaymentScheme.Chaps;

    public ChapsValidator()
    {
        RuleFor(model => model.Request.PaymentScheme)
            .Must((model, scheme) => model.Account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
            .WithMessage("CHAPS not enabled for this account");

        RuleFor(model => model.Request.PaymentScheme)
             .Must((model, scheme) => model.Account.Status == AccountStatus.Live)
             .WithMessage("Account must be Live for CHAPS payments");
    }
}