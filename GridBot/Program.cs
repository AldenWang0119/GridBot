using GridBot.Models;
using GridBot.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var gridConfig = new GridConfig
        {
            Symbol = "SOLUSDT",
            UpperLimit = 267m,
            LowerLimit = 211m,
            GridCount = 10,
            InitialFunds = 100m
        };

        GridServer gridServer = new GridServer();
        gridServer.PlaceSpotGrid(gridConfig);
    }
}