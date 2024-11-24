using System;
using WebSocketSharp;
using GridBot.Models;
using Newtonsoft.Json.Linq;

namespace GridBot.Services
{
    public class WebSocketService
    {
        private WebSocket _webSocket;

        // 構造函數，接受交易對參數
        public WebSocketService(string coin)
        {
            //string wsUrl = $"wss://stream.binance.com:9443/ws/{coin}@trade"; // 交易對
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
            // 解析 JSON 數據
            var json = JObject.Parse(e.Data);

            // 映射到 TickerData 模型
            var tickerData = new TickerData
            {
                Symbol = json["s"]?.ToString(),                             // 交易對
                CurrentPrice = decimal.Parse(json["c"]?.ToString() ?? "0"), // 最新成交價
                BidPrice = decimal.Parse(json["b"]?.ToString() ?? "0"),     // 買一價
                AskPrice = decimal.Parse(json["a"]?.ToString() ?? "0"),     // 賣一價
                HighPrice = decimal.Parse(json["h"]?.ToString() ?? "0"),    // 24 小時最高價
                LowPrice = decimal.Parse(json["l"]?.ToString() ?? "0"),     // 24 小時最低價
                Volume = decimal.Parse(json["v"]?.ToString() ?? "0")        // 24 小時成交量
            };

            // 打印數據作為測試
            Console.WriteLine($"交易對: {tickerData.Symbol}, 現價: {tickerData.CurrentPrice} USDT");
        }

        // 發生錯誤時觸發
        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e) // 注意這裡的 ErrorEventArgs 是 WebSocketSharp 提供的
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
