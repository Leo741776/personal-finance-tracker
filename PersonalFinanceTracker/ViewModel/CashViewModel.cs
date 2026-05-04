using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using PersonalFinanceTracker.Command;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View.Dialog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceTracker.ViewModel
{
    public class CashViewModel : ViewModelBase
    {
        private readonly string _username;
        private readonly CashService _cashService;

        private string _cashAccountId = string.Empty;
        private decimal _cashBalance;
        private decimal _monthlyBudgetLimit;
        private decimal _amountSpentThisMonth;

        public CashViewModel(string username, CashService cashService)
        {
            _username = username;
            _cashService = cashService;

            CurrentMonthTransactions = new ObservableCollection<CashTransaction>();

            AddCashCommand = new RelayCommand(AddCash);
            AddTransactionCommand = new RelayCommand(AddTransaction);
            EditBudgetCommand = new RelayCommand(EditBudget);

            ShowCashFlow7DaysCommand = new RelayCommand(() => BuildCashFlowChart(7));
            ShowCashFlow30DaysCommand = new RelayCommand(() => BuildCashFlowChart(30));
            ShowCashFlow1YearCommand = new RelayCommand(() => BuildCashFlowChart(365));

            _ = LoadAsync();
        }

        public decimal CashBalance
        {
            get => _cashBalance;
            set
            {
                if (_cashBalance == value)
                {
                    return;
                }

                _cashBalance = value;
                OnPropertyChanged();
            }
        }

        public decimal MonthlyBudgetLimit
        {
            get => _monthlyBudgetLimit;
            set
            {
                if (_monthlyBudgetLimit == value)
                {
                    return;
                }

                _monthlyBudgetLimit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOverBudget));
                OnPropertyChanged(nameof(MonthlyBudgetStatus));
            }
        }

        public decimal AmountSpentThisMonth
        {
            get => _amountSpentThisMonth;
            set
            {
                if (_amountSpentThisMonth == value)
                {
                    return;
                }

                _amountSpentThisMonth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOverBudget));
                OnPropertyChanged(nameof(MonthlyBudgetStatus));
            }
        }

        public bool IsOverBudget => AmountSpentThisMonth > MonthlyBudgetLimit;

        public string MonthlyBudgetStatus => IsOverBudget ? "Over Budget" : "Under Budget";

        public ObservableCollection<CashTransaction> CurrentMonthTransactions { get; }

        public ISeries[] CashFlowSeries { get; set; } = Array.Empty<ISeries>();

        public Axis[] CashFlowXAxes { get; set; } = Array.Empty<Axis>();

        public Axis[] CashFlowYAxes { get; set; } = Array.Empty<Axis>();

        public ISeries[] NetCashChangeSeries { get; set; } = Array.Empty<ISeries>();

        public Axis[] NetCashChangeXAxes { get; set; } = Array.Empty<Axis>();

        public Axis[] NetCashChangeYAxes { get; set; } = Array.Empty<Axis>();

        public ICommand AddCashCommand { get; }

        public ICommand AddTransactionCommand { get; }

        public ICommand EditBudgetCommand { get; }

        public ICommand ShowCashFlow7DaysCommand { get; }

        public ICommand ShowCashFlow30DaysCommand { get; }

        public ICommand ShowCashFlow1YearCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                Cash? mainAccount = await _cashService.EnsureDefaultCashAccountAsync(_username);

                if (mainAccount == null)
                {
                    MessageBox.Show("Could not create or load default cash account.");
                    return;
                }

                _cashAccountId = mainAccount.Id;

                List<Cash> accounts = await _cashService.GetCashAccountsAsync(_username);
                CashBalance = accounts.Sum(account => account.Balance);

                List<CashTransaction> transactions =
                    await _cashService.GetCurrentMonthTransactionsAsync(_username);

                CurrentMonthTransactions.Clear();

                foreach (CashTransaction transaction in transactions)
                {
                    CurrentMonthTransactions.Add(transaction);
                }

                AmountSpentThisMonth = await _cashService.GetAmountSpentThisMonthAsync(_username);

                MonthlyBudget budget =
                    await _cashService.GetOrCreateCurrentMonthBudgetAsync(_username);

                MonthlyBudgetLimit = budget.Limit;

                BuildCashFlowChart(7);
                BuildNetCashChangeChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load cash data: {ex.Message}");
            }
        }

        private async void AddCash()
        {
            if (string.IsNullOrWhiteSpace(_cashAccountId))
            {
                MessageBox.Show("No cash account found.");
                return;
            }

            AddCashDialog dialog = new();

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool success = await _cashService.AddCashAsync(
                _username,
                _cashAccountId,
                dialog.Amount,
                dialog.Source);

            if (!success)
            {
                MessageBox.Show("Could not add cash.");
                return;
            }

            await LoadAsync();
        }

        private async void AddTransaction()
        {
            if (string.IsNullOrWhiteSpace(_cashAccountId))
            {
                MessageBox.Show("No cash account found.");
                return;
            }

            AddTransactionDialog dialog = new();

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool success = await _cashService.AddTransactionAsync(
                _username,
                _cashAccountId,
                dialog.Date,
                dialog.Amount,
                dialog.Type,
                dialog.Category,
                dialog.TransactionName);

            if (!success)
            {
                MessageBox.Show("Could not add transaction.");
                return;
            }

            await LoadAsync();
        }

        private async void EditBudget()
        {
            EditBudgetDialog dialog = new(MonthlyBudgetLimit);

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool success = await _cashService.UpdateCurrentMonthBudgetAsync(
                _username,
                dialog.Budget);

            if (!success)
            {
                MessageBox.Show("Could not update monthly budget.");
                return;
            }

            await LoadAsync();
        }

        private void BuildCashFlowChart(int days)
        {
            DateTime startDate = DateTime.Today.AddDays(-days);

            List<CashTransaction> transactions = CurrentMonthTransactions
                .Where(transaction => transaction.Date >= startDate)
                .OrderBy(transaction => transaction.Date)
                .ToList();

            double inflow = (double)transactions
                .Where(transaction => transaction.Amount > 0)
                .Sum(transaction => transaction.Amount);

            double outflow = (double)transactions
                .Where(transaction => transaction.Amount < 0)
                .Sum(transaction => Math.Abs(transaction.Amount));

            CashFlowSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Inflow",
                    Values = new[] { inflow }
                },
                new ColumnSeries<double>
                {
                    Name = "Outflow",
                    Values = new[] { outflow }
                }
            };

            CashFlowXAxes = new[]
            {
                new Axis
                {
                    Labels = new[] { $"{days}D" }
                }
            };

            CashFlowYAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C0")
                }
            };

            OnPropertyChanged(nameof(CashFlowSeries));
            OnPropertyChanged(nameof(CashFlowXAxes));
            OnPropertyChanged(nameof(CashFlowYAxes));
        }

        private void BuildNetCashChangeChart()
        {
            List<CashTransaction> orderedTransactions = CurrentMonthTransactions
                .OrderBy(transaction => transaction.Date)
                .ToList();

            decimal runningTotal = 0m;

            double[] values = orderedTransactions
                .Select(transaction =>
                {
                    runningTotal += transaction.Amount;
                    return (double)runningTotal;
                })
                .ToArray();

            string[] labels = orderedTransactions
                .Select(transaction => transaction.Date.ToString("MM/dd"))
                .ToArray();

            NetCashChangeSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Net Cash Change",
                    Values = values
                }
            };

            NetCashChangeXAxes = new[]
            {
                new Axis
                {
                    Labels = labels
                }
            };

            NetCashChangeYAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C0")
                }
            };

            OnPropertyChanged(nameof(NetCashChangeSeries));
            OnPropertyChanged(nameof(NetCashChangeXAxes));
            OnPropertyChanged(nameof(NetCashChangeYAxes));
        }
    }
}