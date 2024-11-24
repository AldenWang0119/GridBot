using System;
using WebSocketSharp;

namespace GridBot.Services
{
    public class WebSocketService
    {
        private WebSocket _webSocket;

        // 構造函數，接受交易對參數
        public WebSocketService(string coin)
        {
            string wsUrl = $"wss://stream.binance.com:9443/ws/{coin}@trade";
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
            Console.WriteLine("接收到數據:");
            Console.WriteLine(e.Data); // 打印接收到的 JSON 數據
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
