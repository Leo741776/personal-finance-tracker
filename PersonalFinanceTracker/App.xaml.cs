using Google.Cloud.Firestore;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Repository;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View;
using PersonalFinanceTracker.ViewModel;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace PersonalFinanceTracker
{
    public partial class App : Application
    {
        private Authenticator _authenticator = null!;
        private CashService _cashService = null!;
        private SavingsService _savingsService = null!;
        private BrokerageService _brokerageService = null!;
        private AlphaVantageService _alphaVantageService = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string settingsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Secret",
                "appsettings.json");

            if (!File.Exists(settingsPath))
            {
                MessageBox.Show($"Missing settings file: {settingsPath}");
                Shutdown();
                return;
            }

            string settingsJson = File.ReadAllText(settingsPath);

            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(
                settingsJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (settings == null)
            {
                MessageBox.Show("Could not read app settings.");
                Shutdown();
                return;
            }

            string credentialPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Secret",
                settings.Firebase.CredentialsFile);

            if (!File.Exists(credentialPath))
            {
                MessageBox.Show($"Missing Firebase credentials file: {credentialPath}");
                Shutdown();
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.Firebase.ProjectId))
            {
                MessageBox.Show("Firebase project ID is missing.");
                Shutdown();
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.AlphaVantage.ApiKey))
            {
                MessageBox.Show("Alpha Vantage API key is missing.");
                Shutdown();
                return;
            }

            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                credentialPath);

            FirestoreDb database = FirestoreDb.Create(settings.Firebase.ProjectId);

            IUserRepository userRepository = new UserRepository(database);
            ICashRepository cashRepository = new CashRepository(database);
            ISavingsRepository savingsRepository = new SavingsRepository(database);
            IBrokerageRepository brokerageRepository = new BrokerageRepository(database);

            _authenticator = new Authenticator(userRepository);
            _cashService = new CashService(cashRepository);
            _savingsService = new SavingsService(savingsRepository);

            _alphaVantageService = new AlphaVantageService(
                new HttpClient(),
                settings.AlphaVantage.ApiKey);

            _brokerageService = new BrokerageService(
                brokerageRepository,
                _alphaVantageService);

            LoginView loginView = new()
            {
                DataContext = new LoginViewModel(
                    _authenticator,
                    _cashService,
                    _savingsService,
                    _brokerageService,
                    _alphaVantageService)
            };

            loginView.Show();
        }
    }
}