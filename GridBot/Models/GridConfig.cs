namespace GridBot.Models
{
    public class GridConfig
    {
        public decimal UpperLimit { get; set; } // 上限價格
        public decimal LowerLimit { get; set; } // 下限價格
        public int GridCount { get; set; }      // 格子數量
        public decimal InitialFunds { get; set; } // 初始資金
        public string Symbol { get; set; }      // 交易對 (如 BTCUSDT)
    }
}
