namespace ClearBank.DeveloperTest.Types
{
    using ClearBank.DeveloperTest.Exceptions;
    using NodaMoney;

    /// <summary>
    /// Domain model of an account held by a consumer or buisness with fiat funds
    /// </summary>
    public class Account
    {
        public string AccountNumber { get; }
        public Money Balance { get; protected set; }
        public AccountStatus Status { get; protected set; }
        public AllowedPaymentSchemes AllowedPaymentSchemes { get; protected set; }

        public Account(string accountNumber, Money balance, AccountStatus status, AllowedPaymentSchemes schemes) 
        {
            AccountNumber = accountNumber;
            Balance = balance;
            Status = status;
            AllowedPaymentSchemes = schemes;
        }

        public void Credit(Money credit)
        {
            if (credit.Amount < 0) // 0 could be valid account check
            {
                throw new AccountingException(AccountNumber, credit);
            }

            if (Status == AccountStatus.Live || Status == AccountStatus.InboundPaymentsOnly)
            {
                Balance += credit;
            }
            else
            {
                throw new InvalidAccountStateException("Cannot credit account due to account status", AccountNumber);
            }
        }

        public void Debit(Money debit)
        {
            if (debit.Amount < 0)
            {
                throw new AccountingException(AccountNumber, debit);
            }

            if (Status != AccountStatus.Live)
            {
                throw new InvalidAccountStateException("Cannot debit account due to account status", AccountNumber);
            }

            Balance -= debit;
        }
    }
}
