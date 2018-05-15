using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace StockMarketSimulation
{
    public partial class listViewGridView
    {
        public List<Stocks> items;
        public List<Users> users;
        public List<Events> events;

        public listViewGridView(Window1 window, LoadingData load)
        {
            window.InitializeComponent();
            items = load.items;
            users = load.users;
            events = load.events;

            window.listViewStocks.ItemsSource = load.items;

            List<Stocks> itemsPortfolio = new List<Stocks>();
            foreach (var item in items)
            {
                if (item.Holding > 0)
                {
                    itemsPortfolio.Add(item);
                }
            }
            window.listViewPortfolio.ItemsSource = itemsPortfolio;
        }

        internal System.Collections.IEnumerable LoadStocks()
        {
            return items;
        }

        internal System.Collections.IEnumerable LoadPortfolio()
        {
            List<Stocks> itemsPortfolio = new List<Stocks>();
            foreach (var item in items)
            {
                if (item.Holding > 0)
                {
                    itemsPortfolio.Add(item);
                }
            }
            return itemsPortfolio;
        }
        internal System.Collections.IEnumerable LoadEvents()
        {
            return events;
        }

        internal System.Collections.IEnumerable LoadRankings()
        {
            List<Users> usersRanking = new List<Users>(users);
            for (int i = 0; i < usersRanking.Count; i++)
            {
                if (i==0)
                {
                    usersRanking[0].Player = usersRanking[0].Name;
                    usersRanking[0].userColour = true;
                }
                else if (i<usersRanking.Count - 5)
                {
                    usersRanking[i].Player = "JahodaBasic" + i.ToString();
                }
                else
                {
                    usersRanking[i].Player = "JahodaAdvanced" + (i - (usersRanking.Count - 6)).ToString();
                    usersRanking[i].AILevel2 = true;
                }
            }
            foreach (var item in usersRanking)
            {
                double stockWorth = 0;
                for (int i = 0; i < item.StockOwned.Length; i++)
                {
                    if (item.StockOwned[i] > 0)
                    {
                        stockWorth += item.StockOwned[i] * items[i].Price;
                    }
                }
                item.NetWorth = (int)stockWorth + (int)item.Money;
            }
            Quicksort(usersRanking, 0, usersRanking.Count);
            for (int i = 0; i < usersRanking.Count; i++)
            {
                usersRanking[i].Rank = i + 1;
            }
            return usersRanking;
        }
        private static void Swap(List<Users> array, int left, int right)
        {
            Users tmp = array[right];
            array[right] = array[left];
            array[left] = tmp;
        }
        public static void Quicksort(List<Users> array, int left, int right)
        {
            if (left < right)
            {
                int boundary = left;
                for (int i = left + 1; i < right; i++)
                {
                    if (array[i].NetWorth > array[left].NetWorth)
                    {
                        Swap(array, i, ++boundary);
                    }
                }
                Swap(array, left, boundary);
                Quicksort(array, left, boundary);
                Quicksort(array, boundary + 1, right);
            }
        }

        public void BuyStock(int index, int count, int userID)
        {
            bool createNew = false;
            if (userID < 200)
            {
                if (users[0].StockOwned[index] == 0)
                {
                    createNew = true;
                }
            }
            else if (userID >= 200)
            {
                if (users[(userID % 200) + 1].StockOwned[index] == 0)
                {
                    createNew = true;
                }
            }
            if (userID < 200)
            {
                users[0].Money -= items[index].Price * count;
                users[0].StockOwned[index] += count;
                items[index].Count += count;
                items[index].Holding = Math.Round(((users[0].StockOwned[index]) / items[index].SharesOutstanding) * 100, 2);     
            }
            else if (userID >= 200)
            {
                users[(userID % 200) + 1].Money -= items[index].Price * count;
                users[(userID % 200) + 1].StockOwned[index] += count;
                items[index].Count += count;
            }
            WriteData(index, createNew, userID);
        }
        public void SellStock(int index, int count, int userID)
        {
            if (userID < 0)
            {

            }
            else if (userID < 200)
            {
                users[0].Money += items[index].Price * count;
                users[0].StockOwned[index] -= count;
                items[index].Holding = Math.Round(((users[0].StockOwned[index]) / items[index].SharesOutstanding) * 100, 2);
            }
            else if (userID >= 200)
            {
                users[(userID % 200) + 1].Money += items[index].Price * count;
                users[(userID % 200) + 1].StockOwned[index] -= count;
            }
            if (userID >= 0)
            {
                WriteData(index, false, userID);
            }
        }

        private void WriteData(int index, bool createNew, int userID)
        {
            try
            {
                OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + MainWindow.filenameS + ";Persist Security Info=False;");
                string queryString;
                if (userID < 200)
                {
                    queryString = "UPDATE Users SET Users.[Money] = " + Convert.ToInt32(users[0].Money) + " WHERE(((Users.ID) = " + userID.ToString() + "))";
                }
                else
                {
                    queryString = "UPDATE Users SET Users.[Money] = " + Convert.ToInt32(users[(userID % 200) + 1].Money) + " WHERE(((Users.ID) = " + userID.ToString() + "))";
                }

                OleDbCommand cmd = new OleDbCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();

                if (!createNew)
                {
                    queryString = "DELETE From Portfolio WHERE Stock=" + (index + 1).ToString() + " AND User=" + userID + " ";
                }
                else if (createNew)
                {
                    if (userID < 200)
                    {
                        queryString = "INSERT INTO Portfolio ([User], [Stock], [Count]) VALUES ('" + userID.ToString() + "', '" + (index + 1).ToString() + "', '" + users[0].StockOwned[index].ToString() + "')";
                    }
                    else
                    {
                        queryString = "INSERT INTO Portfolio ([User], [Stock], [Count]) VALUES ('" + userID.ToString() + "', '" + (index + 1).ToString() + "', '" + users[(userID % 200) + 1].StockOwned[index].ToString() + "')";
                    }
                }
                else
                {
                    if (userID < 200)
                    {
                        queryString = "UPDATE Portfolio SET Portfolio.[Count]=" + users[0].StockOwned[index].ToString() + " WHERE(((Portfolio.User) = " + userID + ")) AND(((Portfolio.Stock) = " + (index + 1).ToString() + "))";
                    }
                    else
                    {
                        queryString = "UPDATE Portfolio SET Portfolio.[Count]=" + users[(userID % 200) + 1].StockOwned[index].ToString() + " WHERE(((Portfolio.User) = " + userID + ")) AND(((Portfolio.Stock) = " + (index + 1).ToString() + "))";
                    }
                }

                cmd = null;
                cmd = new OleDbCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {

            }
        }

    }
}
