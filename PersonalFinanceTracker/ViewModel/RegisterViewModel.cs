using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceTracker.ViewModel
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly Authenticator _authenticator;
        private readonly CashService _cashService;
        private readonly SavingsService _savingsService;
        private readonly BrokerageService _brokerageService;
        private readonly AlphaVantageService _alphaVantageService;

        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _username = string.Empty;

        public RegisterViewModel(
            Authenticator authenticator,
            CashService cashService,
            SavingsService savingsService,
            BrokerageService brokerageService,
            AlphaVantageService alphaVantageService)
        {
            _authenticator = authenticator;
            _cashService = cashService;
            _savingsService = savingsService;
            _brokerageService = brokerageService;
            _alphaVantageService = alphaVantageService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName == value)
                {
                    return;
                }

                _firstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName == value)
                {
                    return;
                }

                _lastName = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username == value)
                {
                    return;
                }

                _username = value;
                OnPropertyChanged();
            }
        }

        public async Task RegisterAsync(string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            bool success = await _authenticator.RegisterAsync(
                FirstName,
                LastName,
                Username,
                password);

            if (!success)
            {
                MessageBox.Show("Registration failed. Username may already exist.");
                return;
            }

            MessageBox.Show("Registration successful.");

            ShowLogin();
        }

        public void ShowLogin()
        {
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

            Application.Current.Windows
                .OfType<RegisterView>()
                .FirstOrDefault()
                ?.Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}