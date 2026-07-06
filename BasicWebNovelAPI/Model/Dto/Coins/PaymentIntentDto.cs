namespace BasicWebNovelAPI.Model.Dto.Coins
{
    public class PaymentIntentDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
