using ClearBank.DeveloperTest.Types;
using FluentValidation;

namespace ClearBank.DeveloperTest.Validation
{
    public interface IPaymentSchemeValidatorFactory
    {
        IValidator<PaymentValidationModel> Create(PaymentScheme scheme);
    }
}
