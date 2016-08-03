using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace StockMarketSimulation
{
    public class Stocks
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; }
        public double Price { get; set; }
        public double Holding { get; set; }
        public int Count { get; set; }
        public double Value { get; set; }
        public double Change  { get; set; }
        public int ChangePositive { get; set; }
        public double DividendYield { get; set; }
        public double SharesOutstanding { get; set; }
        public double Volume { get; set; }
        public double PriceEarnings { get; set; }
        public List<DateTime> dateHistory { get; set; }
        public List<double> priceHistory { get; set; }
        public int Popularity { get; set; }
    }
}
