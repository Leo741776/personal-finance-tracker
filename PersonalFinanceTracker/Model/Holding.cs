using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    [FirestoreData]
    public class Holding
    {
        public Holding()
        {
        }

        public Holding(
            string ticker,
            string name,
            decimal shares,
            decimal averageCost,
            decimal currentPrice)
        {
            Ticker = ticker;
            Name = name;
            Shares = shares;
            AverageCost = averageCost;
            CurrentPrice = currentPrice;
        }

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Ticker { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Shares { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal AverageCost { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal CurrentPrice { get; set; }

        public decimal TotalCost => Shares * AverageCost;

        public decimal MarketValue => Shares * CurrentPrice;

        public decimal GainLoss => MarketValue - TotalCost;

        public double GainLossPercent =>
            AverageCost == 0
                ? 0
                : (double)((CurrentPrice - AverageCost) / AverageCost * 100);

        public double AllocationPercent { get; set; }
    }
}