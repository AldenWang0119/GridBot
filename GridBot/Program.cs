using GridBot.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        string coin = "btcusdt";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var orderService = new OrderService(configuration);

        //websocket current price
        //var webSocketService = new WebSocketService(coin);
        //webSocketService.Connect();
        //Console.ReadKey();

        //get balance
        //await orderService.GetBalanceList();

        //place order
        //string buyResult = await orderService.PlaceOrderAsync("BTCUSDT", "BUY", 0.001m, 30000.00m, type: "LIMIT");
        //Console.WriteLine(buyResult);

    }
}
