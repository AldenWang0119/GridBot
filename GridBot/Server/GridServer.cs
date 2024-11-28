using GridBot.Models;
using GridBot.Services;
using GridBot.Strategies;
using Microsoft.Extensions.Configuration;

namespace GridBot.Server
{
    public class GridServer
    {
        public void PlaceSpotGrid(GridConfig gridConfig)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var orderService = new OrderService(configuration);
            var gridStrategy = new GridStrategy(gridConfig, orderService);

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
}
