using System;
using WebSocketSharp;
using GridBot.Models;
using Newtonsoft.Json.Linq;

namespace GridBot.Services
{
    public class WebSocketService
    {
        private WebSocket _webSocket;

        // 定義一個事件來通知價格更新
        public event Action<decimal>? OnPriceUpdate;

        // 構造函數，接受交易對參數
        public WebSocketService(string coin)
        {
            string wsUrl = $"wss://stream.binance.com:9443/ws/{coin}@ticker"; // 現價
            _webSocket = new WebSocket(wsUrl);

            // 訂閱事件
            _webSocket.OnMessage += OnMessageReceived;
            _webSocket.OnError += OnError;
            _webSocket.OnClose += OnClose;
        }

        // 啟動 WebSocket 連接
        public void Connect()
        {
            _webSocket.Connect();
            Console.WriteLine("WebSocket 已連接...");
        }

        // 關閉 WebSocket 連接
        public void Close()
        {
            if (_webSocket != null && _webSocket.IsAlive)
            {
                _webSocket.Close();
                Console.WriteLine("WebSocket 已關閉");
            }
        }

        // 接收到數據時觸發
        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                // 解析 JSON 數據
                var json = JObject.Parse(e.Data);

                // 獲取現價
                decimal currentPrice = decimal.Parse(json["c"]?.ToString() ?? "0");

                // 觸發價格更新事件
                OnPriceUpdate?.Invoke(currentPrice);

                // 打印數據作為測試
                Console.WriteLine($"現價更新: {currentPrice} USDT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析 WebSocket 消息失敗: {ex.Message}");
            }
        }

        // 發生錯誤時觸發
        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine($"WebSocket 錯誤: {e.Message}");
        }

        // 關閉連接時觸發
        private void OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine($"WebSocket 已關閉，原因: {e.Reason}");
        }
    }
}
