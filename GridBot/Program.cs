using System;
using GridBot.Services;

class Program
{
    static void Main(string[] args)
    {
        string coin = "solusdt";
        var webSocketService = new WebSocketService(coin);

        webSocketService.Connect();

        Console.WriteLine("按下任意鍵退出...");
        Console.ReadKey();
        webSocketService.Close();
    }
}
