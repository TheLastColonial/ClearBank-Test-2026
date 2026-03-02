namespace ClearBank.DeveloperTest.Types;

/// <summary>
/// Payments rails
/// </summary>
public enum PaymentScheme
{
    /// <summary>
    /// Invalid input
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Faster Payments
    /// </summary>
    /// <seealso href="https://www.wearepay.uk/what-we-do/payment-systems/faster-payment-system/"/>
    /// <remarks>
    /// 15 minute settlements in the UK with partipating banks
    /// </remarks>
    FasterPayments = 1,

    /// <summary>
    /// Bankers' Automated Clearing System
    /// </summary>
    /// <seealso href="https://www.bacs.co.uk/"/>
    /// <remarks>
    /// Traditional payment rails in the UK
    /// </remarks>
    Bacs = 2,

    /// <summary>
    /// Clearing House Automated Payment System
    /// </summary>
    /// <seealso href="https://www.bankofengland.co.uk/payment-and-settlement/chaps"/>
    /// <remarks>
    /// Instant settlement of high value transactions but at increase fee costs
    /// </remarks>
    Chaps = 3
}
