namespace ATM.API.DTOs
{
    public class LoginRequest
    {
        public string CardNumber { get; set; } = string.Empty;
        public string Pin {  get; set; } = string.Empty;
    }
}
