using PersonalFinanceTracker.Model;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace PersonalFinanceTracker.Service
{
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AlphaVantageService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<List<StockSearchResult>> SearchStocksAsync(
            string keywords,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Alpha Vantage API key is missing.");
            }

            if (string.IsNullOrWhiteSpace(keywords) || keywords.Trim().Length < 2)
            {
                return new List<StockSearchResult>();
            }

            string url =
                $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords={Uri.EscapeDataString(keywords)}&apikey={_apiKey}";

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("Note", out JsonElement note))
            {
                throw new InvalidOperationException(note.GetString());
            }

            if (document.RootElement.TryGetProperty("Information", out JsonElement information))
            {
                throw new InvalidOperationException(information.GetString());
            }

            if (!document.RootElement.TryGetProperty("bestMatches", out JsonElement matches))
            {
                return new List<StockSearchResult>();
            }

            List<StockSearchResult> results = new();

            foreach (JsonElement match in matches.EnumerateArray())
            {
                string ticker = GetJsonString(match, "1. symbol");
                string name = GetJsonString(match, "2. name");
                string type = GetJsonString(match, "3. type");
                string region = GetJsonString(match, "4. region");

                if (string.IsNullOrWhiteSpace(ticker) ||
                    string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!region.Contains("United States", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!type.Equals("Equity", StringComparison.OrdinalIgnoreCase) &&
                    !type.Equals("ETF", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                results.Add(new StockSearchResult(ticker, name));
            }

            return results;
        }

        public async Task<decimal> GetLatestPriceAsync(
            string ticker,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Alpha Vantage API key is missing.");
            }

            if (string.IsNullOrWhiteSpace(ticker))
            {
                return 0m;
            }

            string url =
                $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(ticker)}&apikey={_apiKey}";

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            using JsonDocument document = JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty("Note", out JsonElement note))
            {
                throw new InvalidOperationException(note.GetString());
            }

            if (document.RootElement.TryGetProperty("Information", out JsonElement information))
            {
                throw new InvalidOperationException(information.GetString());
            }

            if (!document.RootElement.TryGetProperty("Global Quote", out JsonElement quote))
            {
                return 0m;
            }

            string priceText = GetJsonString(quote, "05. price");

            if (decimal.TryParse(
                    priceText,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal price))
            {
                return price;
            }

            return 0m;
        }

        private static string GetJsonString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement property)
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }
    }
}