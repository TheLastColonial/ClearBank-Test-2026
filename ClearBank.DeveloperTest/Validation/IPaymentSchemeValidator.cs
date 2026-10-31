using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validation
{
    public interface IPaymentSchemeValidator : IValidator<PaymentValidationModel>
    {
        PaymentScheme Scheme { get; }
    }
}
