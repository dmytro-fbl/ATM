namespace ATM.API.DTOs
{
    public class GetBalanceRequest
    {
        public Guid CardId { get; set; }
        public string Pin {  get; set; }
    }
}
