using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketSimulation
{
    public class Events
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string Industry { get; set; }
        public int Stock { get; set; }
        public double Influence { get; set; }
    }
}
