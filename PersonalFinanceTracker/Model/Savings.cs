using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    [FirestoreData]
    public class Savings
    {
        public Savings()
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

        [FirestoreProperty]
        public double InterestRate { get; set; }

        [FirestoreProperty]
        public string LinkedGoalId { get; set; } = string.Empty;
    }

    [FirestoreData]
    public class SavingsGoal
    {
        public SavingsGoal()
        {
        }

        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public string OwnerUsername { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal TargetAmount { get; set; }

        [FirestoreProperty(ConverterType = typeof(DecimalFirestoreConverter))]
        public decimal CurrentAmount { get; set; }

        [FirestoreProperty]
        public DateTime? TargetDate { get; set; }
    }

    [FirestoreData]
    public class SavingsTransaction
    {
        public SavingsTransaction()
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
        public string FromAccountId { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ToSavingsAccountId { get; set; } = string.Empty;
    }

    public class SavingsSummary
    {
        public decimal TotalSavings { get; set; }
        public decimal MonthlySavingsRate { get; set; }
        public double GrowthRate30D { get; set; }
        public double GrowthRate90D { get; set; }
    }
}