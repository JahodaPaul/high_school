using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketSimulation
{
    public class Users
    {
        public int Rank { get; set; }
        public int NetWorth { get; set; }
        public string Player { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public double Money { get; set; }
        public int CompletedQuests { get; set; }
        public bool userColour { get; set; }
        public bool AILevel2 { get; set; }
        public int[] StockOwned = new int[93];
        protected List<int> decisionsNumberOfShares = new List<int>();
        protected List<int> decisionsIndex = new List<int>();
        protected List<string> decisionsType = new List<string>();
        protected List<int> decisionsStopLoss = new List<int>();
        protected List<string> decisionsBuyOrSell = new List<string>();
        protected List<int> decisionsIDUser = new List<int>();

        public virtual void Decision(List<Stocks> items)
        {

        }

        public void PlaceOrders(Window1 window)
        {
            for (int i = 0; i < decisionsNumberOfShares.Count; i++)
            {
                window.placeOrder(decisionsNumberOfShares[i], decisionsIndex[i], decisionsType[i], decisionsStopLoss[i], decisionsBuyOrSell[i], decisionsIDUser[i]);
            }
            decisionsBuyOrSell.Clear();
            decisionsIDUser.Clear();
            decisionsIndex.Clear();
            decisionsNumberOfShares.Clear();
            decisionsStopLoss.Clear();
            decisionsType.Clear();
        }
        public bool DoIEvenHaveTheMoney(Stocks stock,int numberOfShares)
        {
            if (stock.Price * numberOfShares <= this.Money)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
