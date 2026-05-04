using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    [FirestoreData]
    public class Brokerage
    {
        public Brokerage()
        {
        }

        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal CashBalance { get; set; }

        [FirestoreProperty]
        public List<Holding> Holdings { get; set; } = new();

        [FirestoreProperty]
        public List<BrokerageTransaction> Transactions { get; set; } = new();
    }

    [FirestoreData]
    public class BrokerageTransaction
    {
        public BrokerageTransaction()
        {
        }

        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime Date { get; set; }

        [FirestoreProperty]
        public string Ticker { get; set; } = string.Empty;

        [FirestoreProperty]
        public BrokerageTransactionType Type { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Quantity { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Price { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Fees { get; set; }
    }

    public enum BrokerageTransactionType
    {
        Buy,
        Sell,
        Dividend
    }

    public class PortfolioSummary
    {
        public decimal TotalValue { get; set; }
        public decimal TotalGainLoss { get; set; }
        public double TotalGainLossPercent { get; set; }
        public double DayChangePercent { get; set; }
    }
}