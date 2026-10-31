using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validation
{
    internal abstract class PaymentSchemeValidator : AbstractValidator<PaymentValidationModel>, IPaymentSchemeValidator
    {
        protected PaymentSchemeValidator(PaymentScheme scheme, AllowedPaymentSchemes requiredAllowedScheme)
        {
            Scheme = scheme;

            RuleFor(model => model.Account.AllowedPaymentSchemes)
                .Must(allowed => allowed.HasFlag(requiredAllowedScheme))
                .WithErrorCode(PaymentErrorCodes.SchemeNotAllowed);
        }

        public PaymentScheme Scheme { get; }
    }
}
