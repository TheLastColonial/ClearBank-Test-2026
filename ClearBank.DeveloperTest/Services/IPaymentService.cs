using ClearBank.DeveloperTest.Types;
using System.Threading.Tasks;

namespace ClearBank.DeveloperTest.Services;

/// <summary>
/// Service to facilitae payments between <see cref="Account"/>
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Execute a payment
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<MakePaymentResult> MakePayment(MakePaymentRequest request);
}
