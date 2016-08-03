using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketSimulation
{
    class Order
    {
        public int Amount { get; set; }
        public int Index { get; set; }
        public string Type { get; set; }
        public double StopLoss { get; set; }
        public string BuyOrSell { get; set; }
        public int UserId { get; set; }
        public Order(int amount, int index, string type, double stopLoss, string buyOrSell,int userID)
        {
            Amount = amount;
            Index = index;
            Type = type;
            StopLoss = stopLoss;
            BuyOrSell = buyOrSell;
            UserId = userID;   
        }
    }
}
