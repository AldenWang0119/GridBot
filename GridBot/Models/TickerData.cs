namespace GridBot.Models
{
    internal class TickerData
    {
        public string Symbol { get; set; }         // 交易對
        public decimal CurrentPrice { get; set; } // 最新成交價
        public decimal BidPrice { get; set; }     // 買一價
        public decimal AskPrice { get; set; }     // 賣一價
        public decimal HighPrice { get; set; }    // 24 小時內最高價
        public decimal LowPrice { get; set; }     // 24 小時內最低價
        public decimal Volume { get; set; }       // 24 小時內成交量
    }
}
