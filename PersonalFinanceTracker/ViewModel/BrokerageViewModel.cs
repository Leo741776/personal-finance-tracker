using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using PersonalFinanceTracker.Command;
using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PersonalFinanceTracker.ViewModel
{
    public class BrokerageViewModel : ViewModelBase
    {
        private readonly string _username;
        private readonly BrokerageService _brokerageService;
        private readonly AlphaVantageService _alphaVantageService;

        private CancellationTokenSource? _stockSearchCancellationTokenSource;
        private string _brokerageAccountId = string.Empty;
        private decimal _totalPortfolioValue;
        private decimal _totalGainLoss;
        private double _totalGainLossPercent;
        private decimal _dayChange;
        private double _dayChangePercent;
        private string _newStockTicker = string.Empty;
        private string _newStockSearchText = string.Empty;
        private StockSearchResult? _selectedStockSuggestion;
        private bool _isUpdatingFromSelection;
        private decimal _newStockQuantity;
        private decimal _newStockPrice;
        private DateTime? _newStockDate = DateTime.Today;
        private decimal _newStockFees;

        public BrokerageViewModel(
            string username,
            BrokerageService brokerageService,
            AlphaVantageService alphaVantageService)
        {
            _username = username;
            _brokerageService = brokerageService;
            _alphaVantageService = alphaVantageService;

            Holdings = new ObservableCollection<HoldingViewModel>();
            StockSuggestions = new ObservableCollection<StockSearchResult>();

            AddStockCommand = new RelayCommand(AddStock);

            _ = LoadAsync();
        }

        public ObservableCollection<HoldingViewModel> Holdings { get; }

        public ObservableCollection<StockSearchResult> StockSuggestions { get; }

        public decimal TotalPortfolioValue
        {
            get => _totalPortfolioValue;
            set
            {
                if (_totalPortfolioValue == value)
                {
                    return;
                }

                _totalPortfolioValue = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalGainLoss
        {
            get => _totalGainLoss;
            set
            {
                if (_totalGainLoss == value)
                {
                    return;
                }

                _totalGainLoss = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTotalGainLossNegative));
            }
        }

        public double TotalGainLossPercent
        {
            get => _totalGainLossPercent;
            set
            {
                if (Math.Abs(_totalGainLossPercent - value) < 0.001)
                {
                    return;
                }

                _totalGainLossPercent = value;
                OnPropertyChanged();
            }
        }

        public bool IsTotalGainLossNegative => TotalGainLoss < 0;

        public decimal DayChange
        {
            get => _dayChange;
            set
            {
                if (_dayChange == value)
                {
                    return;
                }

                _dayChange = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDayChangeNegative));
            }
        }

        public double DayChangePercent
        {
            get => _dayChangePercent;
            set
            {
                if (Math.Abs(_dayChangePercent - value) < 0.001)
                {
                    return;
                }

                _dayChangePercent = value;
                OnPropertyChanged();
            }
        }

        public bool IsDayChangeNegative => DayChange < 0;

        public string NewStockTicker
        {
            get => _newStockTicker;
            set
            {
                if (_newStockTicker == value)
                {
                    return;
                }

                _newStockTicker = value;
                OnPropertyChanged();
            }
        }

        public string NewStockSearchText
        {
            get => _newStockSearchText;
            set
            {
                if (_newStockSearchText == value)
                {
                    return;
                }

                _newStockSearchText = value;
                OnPropertyChanged();

                if (_isUpdatingFromSelection)
                {
                    return;
                }

                NewStockTicker = value.Trim().ToUpper();

                _ = SearchStockSuggestionsAsync(value);
            }
        }

        public StockSearchResult? SelectedStockSuggestion
        {
            get => _selectedStockSuggestion;
            set
            {
                if (_selectedStockSuggestion == value)
                {
                    return;
                }

                _selectedStockSuggestion = value;
                OnPropertyChanged();

                if (_selectedStockSuggestion == null)
                {
                    return;
                }

                _isUpdatingFromSelection = true;

                NewStockTicker = _selectedStockSuggestion.Ticker;
                NewStockSearchText = _selectedStockSuggestion.DisplayName;

                _isUpdatingFromSelection = false;

                _ = LoadSelectedStockPriceAsync(_selectedStockSuggestion);
            }
        }

        public decimal NewStockQuantity
        {
            get => _newStockQuantity;
            set
            {
                if (_newStockQuantity == value)
                {
                    return;
                }

                _newStockQuantity = value;
                OnPropertyChanged();
            }
        }

        public decimal NewStockPrice
        {
            get => _newStockPrice;
            set
            {
                if (_newStockPrice == value)
                {
                    return;
                }

                _newStockPrice = value;
                OnPropertyChanged();
            }
        }

        public DateTime? NewStockDate
        {
            get => _newStockDate;
            set
            {
                if (_newStockDate == value)
                {
                    return;
                }

                _newStockDate = value;
                OnPropertyChanged();
            }
        }

        public decimal NewStockFees
        {
            get => _newStockFees;
            set
            {
                if (_newStockFees == value)
                {
                    return;
                }

                _newStockFees = value;
                OnPropertyChanged();
            }
        }

        public ISeries[] AllocationSeries { get; set; } = Array.Empty<ISeries>();

        public ICommand AddStockCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                Brokerage? account = await _brokerageService
                    .EnsureDefaultBrokerageAccountAsync(_username);

                if (account == null)
                {
                    MessageBox.Show("Could not create or load default brokerage account.");
                    return;
                }

                _brokerageAccountId = account.Id;

                account.Holdings = _brokerageService.CalculateAllocationPercentages(account);

                PortfolioSummary summary = _brokerageService.CalculatePortfolioSummary(account);

                TotalPortfolioValue = summary.TotalValue;
                TotalGainLoss = summary.TotalGainLoss;
                TotalGainLossPercent = summary.TotalGainLossPercent;

                DayChange = 0m;
                DayChangePercent = summary.DayChangePercent;

                Holdings.Clear();

                foreach (Holding holding in account.Holdings)
                {
                    Holdings.Add(new HoldingViewModel
                    {
                        Ticker = holding.Ticker,
                        Name = holding.Name,
                        Shares = holding.Shares,
                        AverageCost = holding.AverageCost,
                        CurrentPrice = holding.CurrentPrice,
                        AllocationPercent = holding.AllocationPercent
                    });
                }

                BuildAllocationSeries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load brokerage data: {ex.Message}");
            }
        }

        private async Task SearchStockSuggestionsAsync(string searchText)
        {
            _stockSearchCancellationTokenSource?.Cancel();
            _stockSearchCancellationTokenSource = new CancellationTokenSource();

            CancellationToken token = _stockSearchCancellationTokenSource.Token;

            try
            {
                await Task.Delay(400, token);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(searchText) || searchText.Trim().Length < 2)
                {
                    Application.Current.Dispatcher.Invoke(StockSuggestions.Clear);
                    return;
                }

                List<StockSearchResult> results = await _alphaVantageService
                    .SearchStocksAsync(searchText, token);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StockSuggestions.Clear();

                    foreach (StockSearchResult result in results.Take(8))
                    {
                        StockSuggestions.Add(result);
                    }
                });
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Stock search failed: {ex.Message}");
            }
        }

        private async Task LoadSelectedStockPriceAsync(StockSearchResult selectedStock)
        {
            try
            {
                decimal price = await _alphaVantageService.GetLatestPriceAsync(selectedStock.Ticker);

                if (price > 0)
                {
                    selectedStock.CurrentPrice = price;
                    NewStockPrice = price;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load stock price: {ex.Message}");
            }
        }

        private async void AddStock()
        {
            if (string.IsNullOrWhiteSpace(_brokerageAccountId))
            {
                MessageBox.Show("No brokerage account found.");
                return;
            }

            string resolvedTicker = ResolveTicker();

            if (string.IsNullOrWhiteSpace(resolvedTicker))
            {
                MessageBox.Show("Please enter a stock name or ticker.");
                return;
            }

            if (NewStockQuantity <= 0 ||
                NewStockPrice <= 0 ||
                NewStockFees < 0 ||
                NewStockDate == null)
            {
                MessageBox.Show("Please enter valid quantity, price, date, and fees.");
                return;
            }

            bool success = await _brokerageService.AddStockAsync(
                _username,
                _brokerageAccountId,
                resolvedTicker,
                NewStockQuantity,
                NewStockPrice,
                NewStockDate.Value,
                NewStockFees);

            if (!success)
            {
                MessageBox.Show("Could not add stock.");
                return;
            }

            ClearAddStockForm();

            await LoadAsync();
        }

        private string ResolveTicker()
        {
            return SelectedStockSuggestion != null
                ? SelectedStockSuggestion.Ticker
                : NewStockSearchText.Trim().ToUpper();
        }

        private void ClearAddStockForm()
        {
            SelectedStockSuggestion = null;
            NewStockSearchText = string.Empty;
            NewStockTicker = string.Empty;
            NewStockQuantity = 0m;
            NewStockPrice = 0m;
            NewStockDate = DateTime.Today;
            NewStockFees = 0m;

            StockSuggestions.Clear();
        }

        private void BuildAllocationSeries()
        {
            AllocationSeries = Holdings
                .Where(holding => holding.MarketValue > 0)
                .Select(holding => new PieSeries<double>
                {
                    Name = holding.Ticker,
                    Values = new[] { holding.AllocationPercent }
                })
                .ToArray();

            OnPropertyChanged(nameof(AllocationSeries));
        }
    }

    public class HoldingViewModel : ViewModelBase
    {
        private string _ticker = string.Empty;
        private string _name = string.Empty;
        private decimal _shares;
        private decimal _averageCost;
        private decimal _currentPrice;
        private double _allocationPercent;

        public string Ticker
        {
            get => _ticker;
            set
            {
                if (_ticker == value)
                {
                    return;
                }

                _ticker = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        public decimal Shares
        {
            get => _shares;
            set
            {
                if (_shares == value)
                {
                    return;
                }

                _shares = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCost));
                OnPropertyChanged(nameof(MarketValue));
                OnPropertyChanged(nameof(GainLoss));
                OnPropertyChanged(nameof(GainLossPercent));
            }
        }

        public decimal AverageCost
        {
            get => _averageCost;
            set
            {
                if (_averageCost == value)
                {
                    return;
                }

                _averageCost = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalCost));
                OnPropertyChanged(nameof(GainLoss));
                OnPropertyChanged(nameof(GainLossPercent));
            }
        }

        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                if (_currentPrice == value)
                {
                    return;
                }

                _currentPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MarketValue));
                OnPropertyChanged(nameof(GainLoss));
                OnPropertyChanged(nameof(GainLossPercent));
            }
        }

        public decimal TotalCost => Shares * AverageCost;

        public decimal MarketValue => Shares * CurrentPrice;

        public decimal GainLoss => MarketValue - TotalCost;

        public double GainLossPercent =>
            AverageCost == 0
                ? 0
                : (double)((CurrentPrice - AverageCost) / AverageCost * 100);

        public double AllocationPercent
        {
            get => _allocationPercent;
            set
            {
                if (Math.Abs(_allocationPercent - value) < 0.001)
                {
                    return;
                }

                _allocationPercent = value;
                OnPropertyChanged();
            }
        }
    }
}