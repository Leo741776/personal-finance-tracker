using PersonalFinanceTracker.Command;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceTracker.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Authenticator _authenticator;
        private readonly CashService _cashService;
        private readonly SavingsService _savingsService;
        private readonly BrokerageService _brokerageService;
        private readonly AlphaVantageService _alphaVantageService;

        private readonly CashViewModel _cashViewModel;
        private readonly SavingsViewModel _savingsViewModel;
        private readonly BrokerageViewModel _brokerageViewModel;

        private object _currentViewModel;

        public MainViewModel(
            User currentUser,
            Authenticator authenticator,
            CashService cashService,
            SavingsService savingsService,
            BrokerageService brokerageService,
            AlphaVantageService alphaVantageService)
        {
            ArgumentNullException.ThrowIfNull(currentUser);

            _authenticator = authenticator;
            _cashService = cashService;
            _savingsService = savingsService;
            _brokerageService = brokerageService;
            _alphaVantageService = alphaVantageService;

            CurrentUsername = currentUser.Username;
            CurrentUserFullName = $"{currentUser.UserFirstName} {currentUser.UserLastName}";

            _cashViewModel = new CashViewModel(CurrentUsername, _cashService);
            _savingsViewModel = new SavingsViewModel(CurrentUsername, _savingsService);
            _brokerageViewModel = new BrokerageViewModel(
                CurrentUsername,
                _brokerageService,
                _alphaVantageService);

            ShowCashViewCommand = new RelayCommand(ShowCashView);
            ShowSavingsViewCommand = new RelayCommand(ShowSavingsView);
            ShowBrokerageViewCommand = new RelayCommand(ShowBrokerageView);
            LogoutCommand = new RelayCommand(Logout);

            _currentViewModel = _cashViewModel;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentUsername { get; }

        public string CurrentUserFullName { get; }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel == value)
                {
                    return;
                }

                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowCashViewCommand { get; }

        public ICommand ShowSavingsViewCommand { get; }

        public ICommand ShowBrokerageViewCommand { get; }

        public ICommand LogoutCommand { get; }

        private void ShowCashView()
        {
            CurrentViewModel = _cashViewModel;
        }

        private void ShowSavingsView()
        {
            CurrentViewModel = _savingsViewModel;
        }

        private void ShowBrokerageView()
        {
            CurrentViewModel = _brokerageViewModel;
        }

        private void Logout()
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
                .OfType<MainView>()
                .FirstOrDefault()
                ?.Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}