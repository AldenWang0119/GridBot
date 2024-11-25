using GridBot.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var orderService = new OrderService(configuration);

        string buyResult = await orderService.PlaceOrderAsync("BTCUSDT", "BUY", 0.001m, 30000.00m, type: "LIMIT");
        Console.WriteLine(buyResult);
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

}
