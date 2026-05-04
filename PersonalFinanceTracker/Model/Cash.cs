using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    [FirestoreData]
    public class Cash
    {
        public Cash()
        {
        }

        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Balance { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal PendingBalance { get; set; }

        [FirestoreProperty]
        public List<CashTransaction> Transactions { get; set; } = new();
    }

    [FirestoreData]
    public class CashTransaction
    {
        public CashTransaction()
        {
        }

        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime Date { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Amount { get; set; }

        [FirestoreProperty]
        public CashTransactionType Type { get; set; }

        [FirestoreProperty]
        public TransactionCategory Category { get; set; }

        [FirestoreProperty]
        public string MerchantOrSource { get; set; } = string.Empty;

        [FirestoreProperty]
        public string AccountId { get; set; } = string.Empty;
    }

    public enum CashTransactionType
    {
        Income,
        Expense,
        TransferIn,
        TransferOut,
        InvestmentIn,
        InvestmentOut,
        Dividend,
        Refund,
        Adjustment
    }

    public enum TransactionCategory
    {
        Rent,
        Utilities,
        Groceries,
        Transportation,
        Insurance,
        FoodItem,
        Entertainment,
        Shopping,
        Travel,
        Salary,
        Freelance,
        Bonus,
        Savings,
        Investment,
        Dividend,
        Transfer,
        Refund,
        Other
    }

    [FirestoreData]
    public class MonthlyBudget
    {
        public MonthlyBudget()
        {
        }

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Month { get; set; } = string.Empty;

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Limit { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal Spent { get; set; }
    }
}