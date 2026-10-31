using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ClearBank.DeveloperTest.Logging
{
    internal static partial class PaymentServiceLog
    {
        public const int PaymentProcessedId = 1001;
        public const int AccountNotFoundId = 1002;
        public const int PaymentRejectedId = 1003;
        public const int PaymentFailedId = 1004;

        [LoggerMessage(EventId = PaymentProcessedId, Level = LogLevel.Information, Message = "Payment processed")]
        public static partial void PaymentProcessed(this ILogger logger);

        [LoggerMessage(EventId = AccountNotFoundId, Level = LogLevel.Warning, Message = "Payment failed: debtor account not found")]
        public static partial void AccountNotFound(this ILogger logger);

        [LoggerMessage(EventId = PaymentRejectedId, Level = LogLevel.Warning, Message = "Payment rejected, failed rules: {FailedRules}")]
        public static partial void PaymentRejected(this ILogger logger, IReadOnlyCollection<string> failedRules);

        [LoggerMessage(EventId = PaymentFailedId, Level = LogLevel.Error, Message = "Payment failed unexpectedly")]
        public static partial void PaymentFailed(this ILogger logger, Exception exception);
    }
}
