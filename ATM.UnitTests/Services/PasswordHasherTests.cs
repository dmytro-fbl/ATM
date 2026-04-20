using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Infrastructure.Services;
using ATM.Domain.Interfaces;
using ATM.Domain.Interfaces.Services;

namespace ATM.UnitTests.Services
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrueWhenPasswordIsCorrect()
        {
            var password = "1234";

            var hash = _passwordHasher.HashPassword(password);
            var result = _passwordHasher.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWrong()
        {
            var correctPassword = "1234";
            var wrongPassword = "9999";

            var hash = _passwordHasher.HashPassword(correctPassword);

            var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashes_ForSamePassword()
        {
            var password = "1234";

            var hash1 = _passwordHasher.HashPassword(password);
            var hash2 = _passwordHasher.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
            Assert.NotNull(hash1);
            Assert.NotEmpty(hash1);
        }
    }
}
