namespace BasicWebNovelAPI.Model.Coins
{
    public class CoinPackage
    {
        public int Id { get; set; }
        public int CoinsAmount { get; set; }
        public decimal PriceUsd { get; set; }
        public bool IsCustom { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
