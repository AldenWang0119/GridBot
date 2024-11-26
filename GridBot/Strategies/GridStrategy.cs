using GridBot.Models;
using GridBot.Services;
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
        private readonly List<decimal> _gridPrices; // 網格價格列表
        private readonly Dictionary<decimal, bool> _buyFlags; // 買單執行狀態
        private readonly Dictionary<decimal, bool> _sellFlags; // 賣單執行狀態
        private bool _isTerminated; // 是否已終止網格策略

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

            // 檢查止盈和止損條件
            if (currentPrice >= _config.UpperLimit)
            {
                Console.WriteLine($"觸發止盈，價格達到上限：{currentPrice}");
                await TerminateStrategy("止盈");
                return;
            }

            if (currentPrice <= _config.LowerLimit)
            {
                Console.WriteLine($"觸發止損，價格達到下限：{currentPrice}");
                await TerminateStrategy("止損");
                return;
            }

            // 處理網格內的買賣邏輯
            foreach (var gridPrice in _gridPrices)
            {
                if (!_buyFlags[gridPrice] && currentPrice <= gridPrice)
                {
                    Console.WriteLine($"觸發買入網格，價格：{gridPrice}");

                    await ExecuteOrder("BUY", gridPrice);

                    _buyFlags[gridPrice] = true; // 標記為已買入
                    _sellFlags[gridPrice] = false; // 重置賣單狀態
                }

                if (!_sellFlags[gridPrice] && currentPrice >= gridPrice)
                {
                    Console.WriteLine($"觸發賣出網格，價格：{gridPrice}");

                    await ExecuteOrder("SELL", gridPrice);

                    _sellFlags[gridPrice] = true; // 標記為已賣出
                    _buyFlags[gridPrice] = false; // 重置買單狀態
                }
            }
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

                // 下單
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

            // 嘗試平掉所有倉位（可根據你的實現查詢當前倉位並平倉）
            try
            {
                await _orderService.PlaceOrderAsync(_config.Symbol, "SELL", _config.InitialFunds, null, "MARKET");
                Console.WriteLine("所有持倉已平倉。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"平倉失敗，錯誤：{ex.Message}");
            }

            _isTerminated = true; // 標記策略已終止
        }
    }
}
