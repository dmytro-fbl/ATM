using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Infrastructure.Services;

namespace ATM.UnitTests.Validators
{
    public class InputValidatorTests
    {
        private readonly InputValidator _validator;

        public InputValidatorTests()
        {
            _validator = new InputValidator();
        }
        [Theory]
        [InlineData("4111111111111111")]
        public void IsValidCardNumber_ShouldReturnTrue_WhenLuhnCheckPasses(string cardNumber)
        {
            Assert.True(_validator.IsValidCardNumber(cardNumber));
        }

        [Theory]
        [InlineData("4111111111111112")] 
        [InlineData("1234567812345678")]
        public void IsValidCardNumber_ShouldReturnFalse_WhenLuhnCheckFails(string cardNumber)
        {
            Assert.False(_validator.IsValidCardNumber(cardNumber));
        }

        [Theory]
        [InlineData("0000")]
        [InlineData("1234")]
        [InlineData("1111")] 
        public void IsValidPin_ShouldReturnFalse_WhenPinIsTooWeak(string pin)
        {
            Assert.False(_validator.IsValidPin(pin));
        }

        [Theory]
        [InlineData("5927")]
        [InlineData("8163")]
        public void IsValidPin_ShouldReturnTrue_WhenPinIsStrongAnd4Digits(string pin)
        {
            Assert.True(_validator.IsValidPin(pin));
        }

        [Theory]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(20000)]
        public void IsValidWithdrawalAmount_ShouldReturnTrue_WhenValidAndMultipleOf100(decimal amount)
        {
            Assert.True(_validator.IsValidWithdrawalAmount(amount));
        }

        [Theory]
        [InlineData(50)]    
        [InlineData(150)]   
        [InlineData(20001)] 
        [InlineData(0)]     
        public void IsValidWithdrawalAmount_ShouldReturnFalse_WhenInvalidOrOverLimit(decimal amount)
        {
            Assert.False(_validator.IsValidWithdrawalAmount(amount));
        }

    }
}
