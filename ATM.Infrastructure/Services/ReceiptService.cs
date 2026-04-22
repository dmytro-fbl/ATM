using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Interfaces.Services;

namespace ATM.Infrastructure.Services
{
    public class ReceiptService : IReceiptService
    {
        private const int ReceiptWidth = 40;

        public string GenerateAtmReceipt(string cardNumber, string transactionType, decimal amount, decimal balance, string currency = "USD")
        {
            var sb = new StringBuilder();
            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            string transId = GenerateTransactionId();
            string terminalId = "ATM_IPZ-24-2";

            sb.AppendLine(CenterText("ZHYTOMYR RUD BANK"));
            sb.AppendLine(CenterText("TERMINAL RECEIPT"));
            sb.AppendLine(new string('=', ReceiptWidth));

            sb.AppendLine($"DATE: {date}");
            sb.AppendLine($"TERMINAL ID: {terminalId}");
            sb.AppendLine($"TRANSACTION ID: {transId}");
            sb.AppendLine(new string('=', ReceiptWidth));

            sb.AppendLine($"CARD: {MaskCardNumber(cardNumber)}");
            sb.AppendLine($"OPERATION: {transactionType.ToUpper()}");
            sb.AppendLine(new string('-', ReceiptWidth));

            sb.AppendLine($"AMOUNT: {FormatMoney(amount, currency)}");
            sb.AppendLine($"AVAILABLE BALANCE: {FormatMoney(balance, currency)}");
            sb.AppendLine(new string('=', ReceiptWidth));

            sb.AppendLine(CenterText("THANK YOU FOR CHOOSING US!"));
            sb.AppendLine(CenterText("PLEASE KEEP THIS RECEIPT"));

            return sb.ToString();
        }

        private string CenterText(string text)
        {
            if(text.Length >=  ReceiptWidth)
            {
                return text;
            }
                int leftPadding = (ReceiptWidth - text.Length) / 2;
            return text.PadLeft(leftPadding + text.Length).PadRight(ReceiptWidth);
        }

        private string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString().Split('-')[0].ToUpper();
        }
        private string FormatMoney(decimal amount, string currency)
        {
            return $"{amount:F2} {currency}";
        }

        private string MaskCardNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length < 12)
                return "**** **** **** ****";

            string firstFour = number.Substring(0, 4);
            string lastFour = number.Substring(number.Length - 4);
            return $"{firstFour} **** **** {lastFour}";
        }
    }
}
