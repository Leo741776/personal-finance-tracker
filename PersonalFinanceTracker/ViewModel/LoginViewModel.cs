using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace PersonalFinanceTracker.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly Authenticator _authenticator;
        private readonly CashService _cashService;
        private readonly SavingsService _savingsService;
        private readonly BrokerageService _brokerageService;
        private readonly AlphaVantageService _alphaVantageService;

        private string _username = string.Empty;

        public LoginViewModel(
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

        public async Task LoginAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a username and password.");
                return;
            }

            User? user = await _authenticator.LoginAsync(Username, password);

            if (user == null)
            {
                MessageBox.Show("Invalid username or password.");
                return;
            }

            MainView mainView = new()
            {
                DataContext = new MainViewModel(
                    user,
                    _authenticator,
                    _cashService,
                    _savingsService,
                    _brokerageService,
                    _alphaVantageService)
            };

            mainView.Show();

            Application.Current.Windows
                .OfType<LoginView>()
                .FirstOrDefault()
                ?.Close();
        }

        public void ShowRegister()
        {
            RegisterView registerView = new()
            {
                DataContext = new RegisterViewModel(
                    _authenticator,
                    _cashService,
                    _savingsService,
                    _brokerageService,
                    _alphaVantageService)
            };

            registerView.Show();

            Application.Current.Windows
                .OfType<LoginView>()
                .FirstOrDefault()
                ?.Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}