using GridBot.Models;
using GridBot.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GridBot.Strategies
{
    public class GridStrategy
    {
        private readonly GridConfig _config;
        private readonly OrderService _orderService;
        private readonly List<decimal> _gridPrices;
        private readonly Dictionary<decimal, bool> _buyFlags;
        private readonly Dictionary<decimal, bool> _sellFlags;
        private bool _isTerminated;
        private decimal? _lastPrice;

        public GridStrategy(GridConfig config, OrderService orderService)
        {
            _config = config;
            _orderService = orderService;

            _gridPrices = new List<decimal>();
            _buyFlags = new Dictionary<decimal, bool>();
            _sellFlags = new Dictionary<decimal, bool>();
            _isTerminated = false;

            InitializeGridPrices();
        }

        private void InitializeGridPrices()
        {
            decimal step = (_config.UpperLimit - _config.LowerLimit) / _config.GridCount;

            for (int i = 0; i <= _config.GridCount; i++)
            {
                decimal price = _config.LowerLimit + step * i;
                _gridPrices.Add(price);
                _buyFlags[price] = false;
                _sellFlags[price] = false;
            }
        }

        public async void HandlePriceChange(decimal currentPrice)
        {
            if (_isTerminated)
            {
                Console.WriteLine("網格策略已終止，忽略價格更新。");
                return;
            }

            // 初始化時不處理
            if (_lastPrice == null)
            {
                _lastPrice = currentPrice;
                return;
            }

            if (CheckTerminationConditions(currentPrice)) return;

            // 處理價格穿越網格價格時的邏輯
            foreach (var gridPrice in _gridPrices)
            {
                if (await HandleGridCrossing(currentPrice, gridPrice, "BUY", _lastPrice > gridPrice && currentPrice <= gridPrice, _buyFlags))
                {
                    _sellFlags[gridPrice] = false;
                }

                if (await HandleGridCrossing(currentPrice, gridPrice, "SELL", _lastPrice < gridPrice && currentPrice >= gridPrice, _sellFlags))
                {
                    _buyFlags[gridPrice] = false;
                }
            }

            _lastPrice = currentPrice; 
        }

        private bool CheckTerminationConditions(decimal currentPrice)
        {
            if (currentPrice >= _config.UpperLimit)
            {
                Console.WriteLine($"觸發止盈，價格達到上限：{currentPrice}");
                TerminateStrategy("止盈").Wait();
                return true;
            }

            if (currentPrice <= _config.LowerLimit)
            {
                Console.WriteLine($"觸發止損，價格達到下限：{currentPrice}");
                TerminateStrategy("止損").Wait();
                return true;
            }

            return false;
        }

        private async Task<bool> HandleGridCrossing(decimal currentPrice, decimal gridPrice, string side, bool condition, Dictionary<decimal, bool> flags)
        {
            if (!flags[gridPrice] && condition)
            {
                Console.WriteLine($"觸發{side}網格，價格：{gridPrice}");
                await ExecuteOrder(side, gridPrice);
                flags[gridPrice] = true; 
                return true;
            }

            return false;
        }

        private async Task ExecuteOrder(string side, decimal price)
        {
            try
            {
                decimal quantity = Math.Round(_config.InitialFunds / (_gridPrices.Count * price), 2); 

                if (quantity < 0.01m) 
                {
                    Console.WriteLine($"下單失敗：數量 {quantity} 小於最小下單量！");
                    return;
                }

                await _orderService.PlaceOrderAsync(_config.Symbol, side, quantity, price);
                Console.WriteLine($"{side} 訂單成功，價格：{price}，數量：{quantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{side} 訂單失敗，價格：{price}，錯誤：{ex.Message}");
            }
        }

        private async Task TerminateStrategy(string reason)
        {
            Console.WriteLine($"正在平倉並終止網格策略，原因：{reason}");

            try
            {
                var targetBalance = await _orderService.GetTargetAssetBalance(_config.Symbol);


                if (targetBalance != null)
                {

                    if (targetBalance.Value > 0)
                    {
                        await _orderService.PlaceOrderAsync(_config.Symbol, "SELL", Convert.ToInt32(targetBalance), null, "MARKET");
                        Console.WriteLine($"成功平倉，賣出 {targetBalance} {_config.Symbol}。");
                    }
                    else
                    {
                        Console.WriteLine($"沒有持倉可供平倉（{_config.Symbol} 餘額為 0）。");
                    }
                }
                else
                {
                    Console.WriteLine($"無法查詢到 {_config.Symbol} 的餘額。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"平倉失敗，錯誤：{ex.Message}");
            }

            _isTerminated = true;
        }

    }
}
