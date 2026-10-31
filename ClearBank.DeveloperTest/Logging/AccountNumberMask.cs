namespace ClearBank.DeveloperTest.Logging
{
    internal static class AccountNumberMask
    {
        private const string MaskPrefix = "****";
        private const int VisibleCharacters = 4;

        public static string Mask(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                return null;
            }

            if (accountNumber.Length <= VisibleCharacters)
            {
                return MaskPrefix;
            }

            return MaskPrefix + accountNumber.Substring(accountNumber.Length - VisibleCharacters);
        }
    }
}
