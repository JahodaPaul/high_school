using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockMarketSimulation
{
    class TraderLevel2 : Users
    {
        int indexOfStock;
        double BestNumber;
        double forNow;
        List<int> alreadyOwn = new List<int>();
        List<int> alreadySelling = new List<int>();
        public override void Decision(List<Stocks> items)
        {
            
            int range = (this.ID % 200) - 195;
            switch (range)
            {
                case 0:
                    
                    for (int i = 0; i < 18; i++)
                    {
                        forNow = Analysis(items[i]);
                        Method(i);
                    }
                    break;
                case 1:
                    for (int i = 18; i < 36; i++)
                    {
                        forNow = Analysis(items[i]);
                        Method(i);
                    }
                    break;
                case 2:
                    for (int i = 36; i < 55; i++)
                    {
                        forNow = Analysis(items[i]);
                        Method(i);
                    }
                    break;
                case 3:
                    for (int i = 55; i < 74; i++)
                    {
                        forNow = Analysis(items[i]);
                        Method(i);
                    }
                    break;
                case 4:
                    for (int i = 74; i < 93; i++)
                    {
                        forNow = Analysis(items[i]);
                        Method(i);
                    }
                    break;
                default:
                    break;
            }

            if (BestNumber != 0 && !alreadyOwn.Contains(indexOfStock))
            {
                double howMany = HowMany(items[indexOfStock]);
                if (DoIEvenHaveTheMoney(items[indexOfStock], (int)howMany))
                {
                    decisionsNumberOfShares.Add((int)howMany);
                    decisionsIndex.Add(indexOfStock);
                    decisionsType.Add("LimitOrder");
                    decisionsStopLoss.Add(ForHowMuch(items[indexOfStock]));
                    decisionsBuyOrSell.Add("Buy");
                    decisionsIDUser.Add(this.ID);
                    alreadyOwn.Add(indexOfStock);
                }
            }
            if (alreadyOwn.Count > 0)
            {
                for (int i = 0; i < alreadyOwn.Count; i++)
                {
                    if ((this.StockOwned[alreadyOwn[i]] / 20) > 1 && !alreadySelling.Contains(alreadyOwn[i]))
                    {
                        decisionsNumberOfShares.Add((int)(this.StockOwned[alreadyOwn[i]] / 20));
                        decisionsIndex.Add(alreadyOwn[i]);
                        decisionsType.Add("LimitOrder");
                        decisionsStopLoss.Add((int)(items[alreadyOwn[i]].Price * 1.15));
                        decisionsBuyOrSell.Add("Sell");
                        decisionsIDUser.Add(this.ID);
                        alreadySelling.Add(alreadyOwn[i]);
                    }
                }
            }

            //base.Decision(items);
        }
        private void Method(int i)
        {
            if (forNow > BestNumber)
            {
                BestNumber = forNow;
                indexOfStock = i;
            }
        }
        private double Analysis(Stocks stock)
        {
            double change = 1;
            if (stock.priceHistory.Count >= 5)
            {
                change = (stock.priceHistory[stock.priceHistory.Count - 5] / stock.priceHistory[stock.priceHistory.Count - 1]);
            }
            else if (stock.priceHistory.Count >= 2)
            {
                change = (stock.priceHistory[0] / stock.priceHistory[stock.priceHistory.Count - 1]);
            }
            double value = change * (stock.DividendYield + 1);
            return value;
        }
        private double HowMany(Stocks stock)
        {
            if (stock.Price * stock.SharesOutstanding <= this.Money)
            {
                return stock.SharesOutstanding;
            }
            else
            {
                double howMany = this.Money / stock.Price;
                return howMany;
            }
        }
        private int ForHowMuch(Stocks stock)
        {
            double price = stock.Price;
            double multiplier = 1;
            if (stock.priceHistory.Count >= 3)
            {
                if (stock.priceHistory[stock.priceHistory.Count - 1] < stock.priceHistory[stock.priceHistory.Count - 2] && stock.priceHistory[stock.priceHistory.Count - 2] < stock.priceHistory[stock.priceHistory.Count - 3])
                {
                    multiplier = (stock.priceHistory[stock.priceHistory.Count - 1] / stock.priceHistory[stock.priceHistory.Count - 3]) ;
                    multiplier = (multiplier + 1) / 2;
                    price = price * multiplier;
                    return (int)price;
                }
                else if (stock.priceHistory[stock.priceHistory.Count - 1] > stock.priceHistory[stock.priceHistory.Count - 2] && stock.priceHistory[stock.priceHistory.Count - 2] > stock.priceHistory[stock.priceHistory.Count - 3])
                {
                    multiplier = (stock.priceHistory[stock.priceHistory.Count - 1] / stock.priceHistory[stock.priceHistory.Count - 3]);
                    multiplier = (multiplier + 1) / 2;
                    price = price * multiplier;
                    return (int)price;
                }
                else
                {
                    return (int)price;
                }
            }
            else if (stock.priceHistory.Count >= 2)
            {
                multiplier = (stock.priceHistory[stock.priceHistory.Count - 1] / stock.priceHistory[0]);
                multiplier = (multiplier + 2) / 3;
                price = price * multiplier;
                return (int)price;
            }
            else
            {
                return (int)price;
            }
        }

    }
}
