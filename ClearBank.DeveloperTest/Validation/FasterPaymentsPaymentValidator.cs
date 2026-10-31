using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validation
{
    internal sealed class FasterPaymentsPaymentValidator : PaymentSchemeValidator
    {
        public FasterPaymentsPaymentValidator()
            : base(PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments)
        {
            RuleFor(model => model.Account.Balance)
                .GreaterThanOrEqualTo(model => model.Request.Amount)
                .WithErrorCode(PaymentErrorCodes.InsufficientBalance);
        }
    }
}
