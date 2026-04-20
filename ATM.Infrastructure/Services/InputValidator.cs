using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ATM.Infrastructure.Services
{
    public class InputValidator
    {
        public bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) ||
                    cardNumber.Length != 16 || !cardNumber.All(char.IsDigit)) 
                return false;

            int sum = 0;
            for (int i = 0; i < cardNumber.Length; i++)
            {
                int digit = cardNumber[i] - '0';
                if(i %  2 == 0)
                {
                    digit *= 2;
                    if(digit > 9) digit -= 9;
                }
                sum += digit;
            }
            return sum % 10 == 0;            
        }

        public bool IsValidPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
                return false;
            string[] weakPin = { "0000", "1111", "2222", "3333", "4444",
                "5555", "6666", "7777", "8888", "9999", "1234", "4321" };

            return !weakPin.Contains(pin);
        }

        public bool IsValidAmount(decimal amount)
        {
            return amount > 0;
        }

        public bool IsValidWithdrawalAmount(decimal amount)
        {
            if( amount <= 0 || amount > 20000)
                return false;

            return amount % 100 == 0;
        }
    }
}
