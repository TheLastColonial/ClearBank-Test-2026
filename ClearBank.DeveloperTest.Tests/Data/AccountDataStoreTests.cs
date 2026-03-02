using System.Threading.Tasks;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using NodaMoney;

namespace ClearBank.DeveloperTest.Tests.Data;

public class AccountDataStoreTests
{
    private AccountDataStore _store;

    public AccountDataStoreTests()
    {
        _store = new AccountDataStore();
    }

    [Fact]
    public void Assignable()
    {
        Assert.IsAssignableFrom<IAccountRepository>(_store);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("67890")]
    public async Task GetAccount_ReturnsStub(string number)
    {
        var account = await _store.GetAccount(number);
        Assert.NotNull(account);
        Assert.Equal(number, account.AccountNumber);
    }

    [Fact]
    public async Task UpdateAccount_Successful()
    {
        var account = new Account("12345", Money.Zero, AccountStatus.Live, AllowedPaymentSchemes.None);
        await _store.UpdateAccount(account);
    }
}
