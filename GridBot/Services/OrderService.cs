using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GridBot.Services
{
    internal class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public OrderService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();

            // 從配置中讀取 API Key 和 Secret Key
            _apiKey = configuration["Binance:ApiKey"];
            _secretKey = configuration["Binance:SecretKey"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                throw new ArgumentException("API Key 或 Secret Key 未正確配置！");
            }
        }

        public async Task<string> PlaceBuyOrderAsync(string symbol, decimal quantity, decimal price)
        {
            string endpoint = "https://api.binance.com/api/v3/order";
            string queryString = $"symbol={symbol}&side=BUY&type=LIMIT&timeInForce=GTC&quantity={quantity}&price={price}&timestamp={GetTimestamp()}";

            string signature = ComputeSignature(queryString);

            string url = $"{endpoint}?{queryString}&signature={signature}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-MBX-APIKEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PlaceSellOrderAsync(string symbol, decimal quantity, decimal price)
        {
            string endpoint = "https://api.binance.com/api/v3/order";
            string queryString = $"symbol={symbol}&side=SELL&type=LIMIT&timeInForce=GTC&quantity={quantity}&price={price}&timestamp={GetTimestamp()}";

            string signature = ComputeSignature(queryString);
            string url = $"{endpoint}?{queryString}&signature={signature}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-MBX-APIKEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetAccountBalanceAsync()
        {
            string endpoint = "https://api.binance.com/api/v3/account";
            string queryString = $"timestamp={GetTimestamp()}";

            string signature = ComputeSignature(queryString);
            string url = $"{endpoint}?{queryString}&signature={signature}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-MBX-APIKEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private string ComputeSignature(string queryString)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
