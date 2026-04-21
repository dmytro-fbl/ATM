namespace ATM.API.DTOs
{
    public class WithdrawRequest
    {
        public Guid CardId { get; set; }
        public decimal Amount { get; set; }
        public string PinHash { get; set; }
    }
}
