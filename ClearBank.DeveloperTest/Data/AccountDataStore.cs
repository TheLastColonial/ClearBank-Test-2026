using ClearBank.DeveloperTest.Types;
using NodaMoney;
using System.Threading.Tasks;

namespace ClearBank.DeveloperTest.Data;

public class AccountDataStore : IAccountRepository
{
    public Task<Account?> GetAccount(string accountNumber)
    {
        // Access database to retrieve account, code removed for brevity
        var account = new Account(accountNumber, Money.PoundSterling(20), AccountStatus.Live, AllowedPaymentSchemes.Bacs);
        return Task.FromResult(account);
    }

    public Task UpdateAccount(Account account)
    {
        // Update account in database, code removed for brevity
        return Task.CompletedTask;
    }
}
