using GridBot.Models;
using GridBot.Services;
using GridBot.Strategies;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var orderService = new OrderService(configuration);

        // 初始化網格配置
        var gridConfig = new GridConfig
        {
            Symbol = "SOLUSDT",
            UpperLimit = 267m,
            LowerLimit = 211m,
            GridCount = 10,
            InitialFunds = 100m
        };

        var gridStrategy = new GridStrategy(gridConfig, orderService);

        // 啟動 WebSocket 獲取實時價格
        var webSocketService = new WebSocketService(gridConfig.Symbol.ToLower());
        webSocketService.OnPriceUpdate += (currentPrice) =>
        {
            gridStrategy.HandlePriceChange(currentPrice);
        };

        webSocketService.Connect();

        Console.WriteLine("按下任意鍵退出...");
        Console.ReadKey();
        webSocketService.Close();
    }
}
