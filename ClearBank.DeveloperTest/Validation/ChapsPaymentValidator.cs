using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validation
{
    internal sealed class ChapsPaymentValidator : PaymentSchemeValidator
    {
        public ChapsPaymentValidator()
            : base(PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps)
        {
            RuleFor(model => model.Account.Status)
                .Equal(AccountStatus.Live)
                .WithErrorCode(PaymentErrorCodes.AccountNotLive);
        }
    }
}
