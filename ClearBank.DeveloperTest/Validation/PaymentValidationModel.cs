using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Validation
{
    public class PaymentValidationModel
    {
        public PaymentValidationModel(Account account, MakePaymentRequest request)
        {
            Account = account;
            Request = request;
        }

        public Account Account { get; }

        public MakePaymentRequest Request { get; }
    }
}
