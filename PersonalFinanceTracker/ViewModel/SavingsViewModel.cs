using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using PersonalFinanceTracker.Command;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using PersonalFinanceTracker.View.Dialog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceTracker.ViewModel
{
    public class SavingsViewModel : ViewModelBase
    {
        private readonly string _username;
        private readonly SavingsService _savingsService;

        private string _savingsAccountId = string.Empty;
        private decimal _totalSavingsBalance;
        private double _currentApy;

        public SavingsViewModel(string username, SavingsService savingsService)
        {
            _username = username;
            _savingsService = savingsService;

            DepositCommand = new RelayCommand(Deposit);
            EditApyCommand = new RelayCommand(EditApy);

            ShowSavingsBalance90DaysCommand = new RelayCommand(() => BuildSavingsBalanceChart("90D"));
            ShowSavingsBalance1YearCommand = new RelayCommand(() => BuildSavingsBalanceChart("1Y"));
            ShowSavingsBalanceLifetimeCommand = new RelayCommand(() => BuildSavingsBalanceChart("Lifetime"));

            _ = LoadAsync();
        }

        public decimal TotalSavingsBalance
        {
            get => _totalSavingsBalance;
            set
            {
                if (_totalSavingsBalance == value)
                {
                    return;
                }

                _totalSavingsBalance = value;
                OnPropertyChanged();
            }
        }

        public double CurrentApy
        {
            get => _currentApy;
            set
            {
                if (Math.Abs(_currentApy - value) < 0.001)
                {
                    return;
                }

                _currentApy = value;
                OnPropertyChanged();
            }
        }

        public ISeries[] SavingsBalanceSeries { get; set; } = Array.Empty<ISeries>();

        public Axis[] SavingsBalanceXAxes { get; set; } = Array.Empty<Axis>();

        public Axis[] SavingsBalanceYAxes { get; set; } = Array.Empty<Axis>();

        public ICommand DepositCommand { get; }

        public ICommand EditApyCommand { get; }

        public ICommand ShowSavingsBalance90DaysCommand { get; }

        public ICommand ShowSavingsBalance1YearCommand { get; }

        public ICommand ShowSavingsBalanceLifetimeCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                Savings? mainAccount = await _savingsService.EnsureDefaultSavingsAccountAsync(_username);

                if (mainAccount == null)
                {
                    MessageBox.Show("Could not create or load default savings account.");
                    return;
                }

                _savingsAccountId = mainAccount.Id;

                TotalSavingsBalance = await _savingsService.GetTotalSavingsBalanceAsync(_username);
                CurrentApy = await _savingsService.GetAverageApyAsync(_username);

                BuildSavingsBalanceChart("90D");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load savings data: {ex.Message}");
            }
        }

        private async void Deposit()
        {
            if (string.IsNullOrWhiteSpace(_savingsAccountId))
            {
                MessageBox.Show("No savings account found.");
                return;
            }

            DepositDialog dialog = new();

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool success = await _savingsService.DepositAsync(
                _username,
                _savingsAccountId,
                dialog.Amount);

            if (!success)
            {
                MessageBox.Show("Could not deposit to savings.");
                return;
            }

            await LoadAsync();
        }

        private async void EditApy()
        {
            if (string.IsNullOrWhiteSpace(_savingsAccountId))
            {
                MessageBox.Show("No savings account found.");
                return;
            }

            EditApyDialog dialog = new(CurrentApy);

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            bool success = await _savingsService.UpdateApyAsync(
                _username,
                _savingsAccountId,
                dialog.Apy);

            if (!success)
            {
                MessageBox.Show("Could not update APY.");
                return;
            }

            await LoadAsync();
        }

        private void BuildSavingsBalanceChart(string range)
        {
            double current = (double)TotalSavingsBalance;

            double[] values = range switch
            {
                "90D" => new[]
                {
                    Math.Max(0, current - 900),
                    Math.Max(0, current - 700),
                    Math.Max(0, current - 500),
                    Math.Max(0, current - 300),
                    Math.Max(0, current - 100),
                    current
                },
                "1Y" => new[]
                {
                    Math.Max(0, current - 3000),
                    Math.Max(0, current - 2400),
                    Math.Max(0, current - 1800),
                    Math.Max(0, current - 1200),
                    Math.Max(0, current - 600),
                    current
                },
                _ => new[]
                {
                    0d,
                    current * 0.2,
                    current * 0.4,
                    current * 0.6,
                    current * 0.8,
                    current
                }
            };

            string[] labels = range switch
            {
                "90D" => new[] { "90D", "75D", "60D", "45D", "30D", "Now" },
                "1Y" => new[] { "Jan", "Mar", "May", "Jul", "Sep", "Now" },
                _ => new[] { "Start", "Y1", "Y2", "Y3", "Y4", "Now" }
            };

            SavingsBalanceSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Savings Balance",
                    Values = values
                }
            };

            SavingsBalanceXAxes = new[]
            {
                new Axis
                {
                    Labels = labels
                }
            };

            SavingsBalanceYAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("C0")
                }
            };

            OnPropertyChanged(nameof(SavingsBalanceSeries));
            OnPropertyChanged(nameof(SavingsBalanceXAxes));
            OnPropertyChanged(nameof(SavingsBalanceYAxes));
        }
    }
}