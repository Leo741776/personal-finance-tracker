using PersonalFinanceTracker.Model;
using PersonalFinanceTracker.Repository;

namespace PersonalFinanceTracker.Service
{
    public class BrokerageService
    {
        private readonly IBrokerageRepository _brokerageRepository;
        private readonly AlphaVantageService _alphaVantageService;

        public BrokerageService(
            IBrokerageRepository brokerageRepository,
            AlphaVantageService alphaVantageService)
        {
            _brokerageRepository = brokerageRepository;
            _alphaVantageService = alphaVantageService;
        }

        public async Task<Brokerage?> EnsureDefaultBrokerageAccountAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            List<Brokerage> accounts = await _brokerageRepository.GetBrokerageAccountsAsync(username);

            if (accounts.Count > 0)
            {
                return accounts.First();
            }

            Brokerage defaultAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = "Brokerage",
                CashBalance = 0m,
                Holdings = new List<Holding>(),
                Transactions = new List<BrokerageTransaction>()
            };

            bool created = await _brokerageRepository.AddBrokerageAccountAsync(username, defaultAccount);

            return created ? defaultAccount : null;
        }

        public async Task<List<Brokerage>> GetBrokerageAccountsAsync(string username)
        {
            return await _brokerageRepository.GetBrokerageAccountsAsync(username);
        }

        public async Task<Brokerage?> GetBrokerageAccountAsync(string username, string brokerageAccountId)
        {
            return await _brokerageRepository.GetBrokerageAccountAsync(username, brokerageAccountId);
        }

        public async Task<bool> AddBrokerageAccountAsync(
            string username,
            string accountName,
            decimal startingCashBalance)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(accountName) ||
                startingCashBalance < 0)
            {
                return false;
            }

            Brokerage brokerageAccount = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Name = accountName,
                CashBalance = startingCashBalance,
                Holdings = new List<Holding>(),
                Transactions = new List<BrokerageTransaction>()
            };

            return await _brokerageRepository.AddBrokerageAccountAsync(username, brokerageAccount);
        }

        public async Task<bool> AddStockAsync(
            string username,
            string brokerageAccountId,
            string ticker,
            decimal quantity,
            decimal price,
            DateTime date,
            decimal fees)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId) ||
                string.IsNullOrWhiteSpace(ticker) ||
                quantity <= 0 ||
                price <= 0 ||
                fees < 0)
            {
                return false;
            }

            string normalizedTicker = ticker.Trim().ToUpper();

            Holding holding = new()
            {
                OwnerUsername = username,
                Ticker = normalizedTicker,
                Name = normalizedTicker,
                Shares = quantity,
                AverageCost = price,
                CurrentPrice = price
            };

            BrokerageTransaction transaction = new()
            {
                Id = Guid.NewGuid().ToString(),
                OwnerUsername = username,
                Date = ToUtcDateTime(date),
                Ticker = normalizedTicker,
                Type = BrokerageTransactionType.Buy,
                Quantity = quantity,
                Price = price,
                Fees = fees
            };

            bool holdingAdded = await _brokerageRepository.AddHoldingAsync(
                username,
                brokerageAccountId,
                holding);

            if (!holdingAdded)
            {
                return false;
            }

            return await _brokerageRepository.AddBrokerageTransactionAsync(
                username,
                brokerageAccountId,
                transaction);
        }

        public async Task<Brokerage?> RefreshHoldingPricesAsync(
            string username,
            string brokerageAccountId)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId))
            {
                return null;
            }

            Brokerage? brokerage = await _brokerageRepository.GetBrokerageAccountAsync(
                username,
                brokerageAccountId);

            if (brokerage == null)
            {
                return null;
            }

            foreach (Holding holding in brokerage.Holdings)
            {
                if (string.IsNullOrWhiteSpace(holding.Ticker))
                {
                    continue;
                }

                decimal latestPrice = await _alphaVantageService.GetLatestPriceAsync(holding.Ticker);

                if (latestPrice > 0)
                {
                    holding.CurrentPrice = latestPrice;
                    holding.OwnerUsername = username;
                }
            }

            await _brokerageRepository.UpdateBrokerageAccountAsync(username, brokerage);

            brokerage.Holdings = CalculateAllocationPercentages(brokerage);

            return brokerage;
        }

        public PortfolioSummary CalculatePortfolioSummary(Brokerage brokerage)
        {
            decimal holdingsValue = brokerage.Holdings.Sum(holding => holding.MarketValue);

            decimal positiveCashBalance = brokerage.CashBalance > 0
                ? brokerage.CashBalance
                : 0m;

            decimal totalValue = holdingsValue + positiveCashBalance;
            decimal totalCost = brokerage.Holdings.Sum(holding => holding.TotalCost);
            decimal totalGainLoss = brokerage.Holdings.Sum(holding => holding.GainLoss);

            double totalGainLossPercent = totalCost == 0
                ? 0
                : (double)(totalGainLoss / totalCost * 100);

            return new PortfolioSummary
            {
                TotalValue = totalValue,
                TotalGainLoss = totalGainLoss,
                TotalGainLossPercent = totalGainLossPercent,
                DayChangePercent = 0
            };
        }

        public decimal CalculateTotalPortfolioValue(Brokerage brokerage)
        {
            decimal holdingsValue = brokerage.Holdings.Sum(holding => holding.MarketValue);

            decimal positiveCashBalance = brokerage.CashBalance > 0
                ? brokerage.CashBalance
                : 0m;

            return holdingsValue + positiveCashBalance;
        }

        public decimal CalculateTotalGainLoss(Brokerage brokerage)
        {
            return brokerage.Holdings.Sum(holding => holding.GainLoss);
        }

        public double CalculateTotalGainLossPercent(Brokerage brokerage)
        {
            decimal totalCost = brokerage.Holdings.Sum(holding => holding.TotalCost);

            if (totalCost == 0)
            {
                return 0;
            }

            decimal totalGainLoss = CalculateTotalGainLoss(brokerage);

            return (double)(totalGainLoss / totalCost * 100);
        }

        public List<Holding> CalculateAllocationPercentages(Brokerage brokerage)
        {
            decimal totalHoldingValue = brokerage.Holdings.Sum(holding => holding.MarketValue);

            foreach (Holding holding in brokerage.Holdings)
            {
                holding.AllocationPercent = totalHoldingValue == 0
                    ? 0
                    : (double)(holding.MarketValue / totalHoldingValue * 100);
            }

            return brokerage.Holdings;
        }

        public async Task<bool> UpdateHoldingPriceAsync(
            string username,
            string brokerageAccountId,
            string ticker,
            decimal currentPrice)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(brokerageAccountId) ||
                string.IsNullOrWhiteSpace(ticker) ||
                currentPrice < 0)
            {
                return false;
            }

            Brokerage? brokerage = await _brokerageRepository.GetBrokerageAccountAsync(
                username,
                brokerageAccountId);

            if (brokerage == null)
            {
                return false;
            }

            Holding? holding = brokerage.Holdings.FirstOrDefault(existingHolding =>
                existingHolding.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));

            if (holding == null)
            {
                return false;
            }

            holding.OwnerUsername = username;
            holding.CurrentPrice = currentPrice;

            return await _brokerageRepository.UpdateBrokerageAccountAsync(username, brokerage);
        }

        private static DateTime ToUtcDateTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                return dateTime.ToUniversalTime();
            }

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}