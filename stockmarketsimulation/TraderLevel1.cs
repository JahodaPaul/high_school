using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketSimulation
{
    class TraderLevel1 : Users
    {
        // primitivni obchodnik na bázi pseudonahodneho vyberu prodeje a nakupu akcií
        public static readonly Random random = new Random();
        public static readonly object syncLock = new object();
        public override void Decision(List<Stocks> items)
        {
            int randomNumber;
            int buyOrSell = 0;
            double number;
            double multiplyBy;
            double price;
            for (int i = 0; i < 2; i++)
            {
                randomNumber = GetRandomNumber(0, 93);
                //int buyOrSell = GetRandomNumber(0, 1);
                number = GetRandomNumber(80, 121);
                multiplyBy = number / 100;
                price = items[randomNumber].Price * multiplyBy;
                if (buyOrSell == 0)
                {
                    if (DoIEvenHaveTheMoney(items[randomNumber],5))
                    {
                        decisionsNumberOfShares.Add(5);
                        decisionsIndex.Add(randomNumber);
                        decisionsType.Add("LimitOrder");
                        decisionsStopLoss.Add((int)price);
                        decisionsBuyOrSell.Add("Buy");
                        decisionsIDUser.Add(this.ID); 
                    }
                }
            }
            for (int i = 0; i < 93; i++)
            {
                if (this.StockOwned[i] != 0)
                {
                    int sellIt = GetRandomNumber(0, 8);
                    if (sellIt == 0)
                    {
                        number = GetRandomNumber(80, 121);
                        multiplyBy = number / 100;
                        price = items[i].Price * multiplyBy;
                        decisionsNumberOfShares.Add(5);
                        decisionsIndex.Add(i);
                        decisionsType.Add("LimitOrder");
                        decisionsStopLoss.Add((int)price);
                        decisionsBuyOrSell.Add("Sell");
                        decisionsIDUser.Add(this.ID);
                    }
                }
            }
            //base.Decision();
        }
        public static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
    }
}
