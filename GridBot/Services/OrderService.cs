using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GridBot.Services
{
    public class OrderService
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

        public async Task<string?> GetTargetAssetBalance(string targetAsset)
        {
            try
            {
                Console.WriteLine($"正在查詢 {targetAsset} 資產餘額...");
                string accountBalance = await GetAccountBalanceAsync();

                var json = JObject.Parse(accountBalance);

                // 查找目標資產的餘額
                var targetBalance = json["balances"]?
                    .FirstOrDefault(b => b["asset"]?.ToString() == targetAsset);

                if (targetBalance != null)
                {
                    string free = targetBalance["free"]?.ToString() ?? "0";
                    string locked = targetBalance["locked"]?.ToString() ?? "0";

                    return free;
                }
                else
                {
                    Console.WriteLine($"未找到目標資產 {targetAsset} 的餘額資訊！");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查詢 {targetAsset} 資產餘額失敗: {ex.Message}");
                return "0"; 
            }
        }

        public async Task<string> PlaceOrderAsync(string symbol, string side, decimal quantity, decimal? price = null, string type = "LIMIT")
        {
            string endpoint = "https://api.binance.com/api/v3/order";
            string timeInForce = "GTC"; 
            string queryString = $"symbol={symbol}&side={side}&type={type}&quantity={quantity}&timestamp={GetTimestamp()}";

            if (type == "LIMIT" && price.HasValue)
            {
                queryString += $"&timeInForce={timeInForce}&price={price.Value}";
            }

            string signature = ComputeSignature(queryString);
            string url = $"{endpoint}?{queryString}&signature={signature}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-MBX-APIKEY", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"下單失敗: {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetAccountBalanceAsync()
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
