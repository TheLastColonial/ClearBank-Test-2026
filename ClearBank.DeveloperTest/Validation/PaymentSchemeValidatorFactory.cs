using ClearBank.DeveloperTest.Types;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClearBank.DeveloperTest.Validation
{
    internal sealed class PaymentSchemeValidatorFactory : IPaymentSchemeValidatorFactory
    {
        private static readonly IValidator<PaymentValidationModel> AcceptAll = new InlineValidator<PaymentValidationModel>();

        private readonly IReadOnlyDictionary<PaymentScheme, IPaymentSchemeValidator> _validatorsByScheme;

        public PaymentSchemeValidatorFactory(IEnumerable<IPaymentSchemeValidator> schemeValidators)
        {
            ArgumentNullException.ThrowIfNull(schemeValidators);

            var validators = schemeValidators.ToList();

            var misconfiguredSchemes = Enum.GetValues<PaymentScheme>()
                .Union(validators.Select(validator => validator.Scheme))
                .Where(scheme => validators.Count(validator => validator.Scheme == scheme) != 1)
                .ToList();

            if (misconfiguredSchemes.Any())
            {
                throw new ArgumentException(
                    $"Exactly one validator is required for each payment scheme. Misconfigured: {string.Join(", ", misconfiguredSchemes)}.",
                    nameof(schemeValidators));
            }

            _validatorsByScheme = validators.ToDictionary(validator => validator.Scheme);
        }

        public IValidator<PaymentValidationModel> Create(PaymentScheme scheme) =>
            _validatorsByScheme.TryGetValue(scheme, out var validator) ? validator : AcceptAll;
    }
}
