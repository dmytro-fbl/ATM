using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using ATM.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ATM.Infrastructure.Services
{
    public class ReceiptService : IReceiptService
    {
        private const int ReceiptWidth = 40;

        private readonly string _bankName;
        private readonly string _terminalId;
        private readonly string _defaultCurrency;

        public ReceiptService(IConfiguration configuration)
        {
            _bankName = configuration["Receipt:BankName"];
            _terminalId = configuration["Receipt:TerminalId"];
            _defaultCurrency = configuration["Receipt:Currency"];
        }

        public string GenerateAtmReceipt(string cardNumber, string transactionType, decimal amount, decimal balance, string currency = "USD")
        {
            var sb = new StringBuilder();
            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            string transId = GenerateTransactionId();
            string terminalId = _terminalId;

            sb.AppendLine(CenterText(_bankName));
            sb.AppendLine(CenterText("TERMINAL RECEIPT"));
            sb.AppendLine(new string('=', ReceiptWidth));

            sb.AppendLine($"DATE: {date}");
            sb.AppendLine($"TERMINAL ID: {terminalId}");
            sb.AppendLine($"TRANSACTION ID: {transId}");
            sb.AppendLine(new string('=', ReceiptWidth));

            sb.AppendLine($"CARD: {MaskCardNumber(cardNumber)}");
            sb.AppendLine($"OPERATION: {transactionType.ToUpper()}");
            sb.AppendLine(new string('-', ReceiptWidth));

            var currencyToUse = string.IsNullOrWhiteSpace(currency) || currency == "USD" ? _defaultCurrency : currency;

            sb.AppendLine($"AMOUNT: {FormatMoney(amount, currencyToUse)}");
            sb.AppendLine($"AVAILABLE BALANCE: {FormatMoney(balance, currencyToUse)}");
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
            return $"{amount.ToString("F2", CultureInfo.InvariantCulture)} {currency}";
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
