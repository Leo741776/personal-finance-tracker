namespace PersonalFinanceTracker.Model
{
    public class StockSearchResult
    {
        public StockSearchResult()
        {
        }

        public StockSearchResult(string ticker, string name, decimal currentPrice = 0)
        {
            Ticker = ticker;
            Name = name;
            CurrentPrice = currentPrice;
        }

        public string Ticker { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal CurrentPrice { get; set; }

        public string DisplayName => $"{Name} ({Ticker})";
    }
}