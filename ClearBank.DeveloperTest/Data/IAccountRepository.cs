namespace ClearBank.DeveloperTest.Data;

using ClearBank.DeveloperTest.Types;
using System.Threading.Tasks;

/// <summary>
/// Gets <see cref="Account"/> from store
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets <see cref="Account"/> by number
    /// </summary>
    /// <param name="accountNumber">Human readable number of the account</param>
    /// <returns>Account if present or null if none found</returns>
    public Task<Account?> GetAccount(string accountNumber);
    public Task UpdateAccount(Account account);
}
