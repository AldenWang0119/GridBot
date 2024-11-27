using System;
using WebSocketSharp;
using GridBot.Models;
using Newtonsoft.Json.Linq;

namespace GridBot.Services
{
    public class WebSocketService
    {
        private WebSocket _webSocket;

        public event Action<decimal>? OnPriceUpdate;

        public WebSocketService(string coin)
        {
            string wsUrl = $"wss://stream.binance.com:9443/ws/{coin}@ticker";
            _webSocket = new WebSocket(wsUrl);

            _webSocket.OnMessage += OnMessageReceived;
            _webSocket.OnError += OnError;
            _webSocket.OnClose += OnClose;
        }

        public void Connect()
        {
            _webSocket.Connect();
            Console.WriteLine("WebSocket 已連接...");
        }

        public void Close()
        {
            if (_webSocket != null && _webSocket.IsAlive)
            {
                _webSocket.Close();
                Console.WriteLine("WebSocket 已關閉");
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                var json = JObject.Parse(e.Data);

                decimal currentPrice = decimal.Parse(json["c"]?.ToString() ?? "0");

                OnPriceUpdate?.Invoke(currentPrice);

                Console.WriteLine($"現價更新: {currentPrice} USDT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析 WebSocket 消息失敗: {ex.Message}");
            }
        }

        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine($"WebSocket 錯誤: {e.Message}");
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine($"WebSocket 已關閉，原因: {e.Reason}");
        }
    }
}
