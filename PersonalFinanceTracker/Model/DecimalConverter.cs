using Google.Cloud.Firestore;

namespace PersonalFinanceTracker.Model
{
    public class DecimalFirestoreConverter : IFirestoreConverter<decimal>
    {
        public object ToFirestore(decimal value)
        {
            return Convert.ToDouble(value);
        }

        public decimal FromFirestore(object value)
        {
            return value switch
            {
                double doubleValue => Convert.ToDecimal(doubleValue),
                long longValue => Convert.ToDecimal(longValue),
                int intValue => Convert.ToDecimal(intValue),
                _ => 0m
            };
        }
    }
}