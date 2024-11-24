using GridBot.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        await GetBalance();

        SubscribeToPriceUpdates();
    }

    private static void SubscribeToPriceUpdates()
    {
        string coin = "solusdt";
        var webSocketService = new WebSocketService(coin);

        webSocketService.Connect();

        Console.WriteLine("按下任意鍵退出...");
        Console.ReadKey();
        webSocketService.Close();
    }

    private static async Task GetBalance()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string apiKey = configuration["Binance:ApiKey"];
        string secretKey = configuration["Binance:SecretKey"];

        var orderService = new OrderService(apiKey, secretKey);

        try
        {
            Console.WriteLine("正在查詢帳戶餘額...");
            string accountBalance = await orderService.GetAccountBalanceAsync();

            var json = JObject.Parse(accountBalance);

            // 過濾出有餘額的資產
            var balances = json["balances"]?
                .Where(b => b["free"] != null && decimal.TryParse(b["free"]?.ToString(), out decimal free) && free > 0)
                .ToList();

            if (balances != null && balances.Count > 0)
            {
                Console.WriteLine("有餘額的資產列表：");
                foreach (var balance in balances)
                {
                    string asset = balance["asset"]?.ToString() ?? "未知資產";
                    string free = balance["free"]?.ToString() ?? "0";
                    string locked = balance["locked"]?.ToString() ?? "0";

                    Console.WriteLine($"資產: {asset}, 可用餘額: {free}, 鎖定餘額: {locked}");
                }
            }
            else
            {
                Console.WriteLine("未找到有餘額的資產！");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"查詢帳戶餘額失敗: {ex.Message}");
        }
    }
}
