using ClearBank.DeveloperTest.Logging;
using Xunit;

namespace ClearBank.DeveloperTest.Tests.Logging
{
    public class AccountNumberMaskTests
    {
        [Theory]
        [InlineData("12345678", "****5678")]
        [InlineData("1234567890123456", "****3456")]
        [InlineData("12345", "****2345")]
        public void Only_the_last_four_characters_are_kept(string accountNumber, string expected)
        {
            Assert.Equal(expected, AccountNumberMask.Mask(accountNumber));
        }

        [Theory]
        [InlineData("1234")]
        [InlineData("12")]
        [InlineData("1")]
        public void Short_account_numbers_are_fully_masked(string accountNumber)
        {
            Assert.Equal("****", AccountNumberMask.Mask(accountNumber));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Missing_account_numbers_stay_missing(string accountNumber)
        {
            Assert.Null(AccountNumberMask.Mask(accountNumber));
        }
    }
}
