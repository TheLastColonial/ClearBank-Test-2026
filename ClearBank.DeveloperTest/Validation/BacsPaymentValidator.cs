using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Validation
{
    internal sealed class BacsPaymentValidator : PaymentSchemeValidator
    {
        public BacsPaymentValidator()
            : base(PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs)
        {
        }
    }
}
