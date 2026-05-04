namespace PersonalFinanceTracker.Model
{
    public class AppSettings
    {
        public FirebaseSettings Firebase { get; set; } = new();
        public AlphaVantageSettings AlphaVantage { get; set; } = new();
    }

    public class FirebaseSettings
    {
        public string ProjectId { get; set; } = string.Empty;
        public string CredentialsFile { get; set; } = string.Empty;
    }

    public class AlphaVantageSettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}