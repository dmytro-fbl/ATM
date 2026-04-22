using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Infrastructure.Services;

namespace ATM.UnitTests.Services
{
    public class ReceiptServiceTests
    {
        private readonly ReceiptService _receiptService;

        public ReceiptServiceTests()
        {
            _receiptService = new ReceiptService();
        }

        [Fact]
        public void GenerateAtmReceipt_ShouldMaskCardNumberCorrectly()
        {
            string fullCardNumber = "1234567812345678";

            var receipt = _receiptService.GenerateAtmReceipt(fullCardNumber, "Зняття", 500, 1500);

            Assert.Contains("1234 **** **** 5678", receipt);
            Assert.DoesNotContain("56781234", receipt);
        }
        [Fact]
        public void GenerateAtmReceipt_ShouldHandleShortCardNumber()
        {
            string shortNumber = "123"; 

            var receipt = _receiptService.GenerateAtmReceipt(shortNumber, "Зняття", 100, 100);
            Assert.Contains("**** **** **** ****", receipt); // Має повернути стандартну маску
        }

        [Theory]
        [InlineData(100.50, "100.50 USD")]
        [InlineData(2000.00, "2000.00 USD")]
        [InlineData(0.00, "0.00 USD")]
        public void GenerateAtmReceipt_ShouldFormatMoneyCorrectly(decimal amount, string expectedText)
        {
            var receipt = _receiptService.GenerateAtmReceipt("1111222233334444", "Поповнення", amount, 5000);
            Assert.Contains(expectedText, receipt);
        }

        [Fact]
        public void GenerateAtmReceipt_ShouldContainCorrectHeaders()
        {
            var receipt = _receiptService.GenerateAtmReceipt("1111222233334444", "Зняття", 100, 100);
            Assert.Contains("ZHYTOMYR RUD BANK", receipt);
            Assert.Contains("ATM_IPZ-24-2", receipt);
            Assert.Contains("THANK YOU FOR CHOOSING US!", receipt);
        }
    }
}
