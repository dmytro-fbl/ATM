namespace ATM.API.DTOs
{
    public class DepositRequest
    {
        public Guid CardId {  get; set; }
        public Dictionary<int, int> Banknotes { get; set; }
    }
}
