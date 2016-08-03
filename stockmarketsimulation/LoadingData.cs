using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace StockMarketSimulation
{
    public class LoadingData
    {
        public List<Stocks> items;
        public List<Users> users;
        public List<Events> events;
        string UIname = "GrznfWCi5a";
        string Filename { get; set; }

        public LoadingData(String filename, int userID)
        {
            items = new List<Stocks>();
            users = new List<Users>();
            events = new List<Events>();

            Filename = filename;

            #region LoadingStocks
            OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Persist Security Info=False;");
            string queryString = "SELECT * FROM Stocks";
            OleDbCommand command = new OleDbCommand(queryString, connection);
            connection.Open();
            OleDbDataReader reader = command.ExecuteReader();

            
            while (reader.Read())
            {
                items.Add(new Stocks() { ID = reader.GetInt32(0), Name = reader.GetString(1), Price = reader.GetDouble(2), Industry = reader.GetString(3), Country = reader.GetString(4), SharesOutstanding = reader.GetDouble(5), Volume = reader.GetDouble(6), PriceEarnings = reader.GetDouble(7), Holding = 0, Value = 0, Change = 0, DividendYield = reader.GetDouble(8), dateHistory = new List<DateTime>(), priceHistory = new List<double>() });
            }
            connection.Close();
            for (int i = 0; i < items.Count - 1; i++)
            {
                int j = i + 1;
                Stocks temporary = items[j];
                while (j > 0 && temporary.ID < items[j - 1].ID)
                {
                    items[j] = items[j - 1];
                    j--;
                }
                items[j] = temporary;
            }
            foreach (var item in items)
            {
                item.ID = item.ID - 1;
            }
            #endregion

            #region LoadingStockHistory
            queryString = "SELECT PriceHistory.* FROM Stocks INNER JOIN PriceHistory ON Stocks.ID = PriceHistory.Stock WHERE User=" + userID.ToString();
            command = new OleDbCommand(queryString, connection);
            connection.Open();
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                items[reader.GetInt32(1) - 1].dateHistory.Add(reader.GetDateTime(2));
                items[reader.GetInt32(1) - 1].priceHistory.Add(reader.GetDouble(3));
            }
            connection.Close();
            #endregion

            #region LoadingUser
            queryString = "SELECT Users.* FROM Users WHERE(((Users.ID) = " + userID.ToString() + "))";
            command = new OleDbCommand(queryString, connection);
            connection.Open();
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new Users());
                users[0].ID = reader.GetInt32(0);
                users[0].Name = reader.GetString(1);
                users[0].Money = reader.GetDouble(2);
                users[0].CompletedQuests = reader.GetInt32(3);
            }
            #endregion

            #region LoadingAIUsers
            queryString = "SELECT Users.* FROM Users WHERE(((Users.ID) = " + ((userID * 200) + 200).ToString() + "))";
            command = new OleDbCommand(queryString, connection);
            reader = command.ExecuteReader();
            bool exist = false;
            while (reader.Read())
            {
                exist = true;
            }
            // v pripade ze obchodnici k danemu uzivatelskem uctu existuje tak je nacte
            if (exist == true)
            {
                queryString = "SELECT Users.* FROM Users WHERE(((Users.ID) > " + ((userID * 200) + 199) + ") AND (Users.ID) < " + ((userID * 200) + 400) + ") ORDER BY(Users.ID)";
                command = new OleDbCommand(queryString, connection);
                reader = command.ExecuteReader();
                int counter = 1;
                while (reader.Read())
                {
                    if (counter < 196)
                    {
                        users.Add(new TraderLevel1());
                    }
                    else
                    {
                        users.Add(new TraderLevel2());
                    }                    
                    //users.Add(new Users());
                    users[counter].ID = reader.GetInt32(0);
                    users[counter].Name = reader.GetString(1);
                    users[counter].Money = reader.GetDouble(2);
                    users[counter].CompletedQuests = reader.GetInt32(3);
                    counter++;
                }

            }
            // v pripade ze obchodnici neexistuje tak je vytvori
            else if (exist == false)
            {
                for (int i = ((userID * 200) + 200); i < ((userID * 200) + 400); i++)
                {
                    queryString = "INSERT INTO Users VALUES (" + i + ",'" + (UIname + i.ToString()) + "','" + 50000 + "','" + 0 + "')";
                    command = new OleDbCommand(queryString, connection);
                    ExecuteNonQuery(queryString);
                }
                for (int i = 0; i < 200; i++)
                {
                    if (i < 195)
                    {
                        users.Add(new TraderLevel1());
                    }
                    else
                    {
                        users.Add(new TraderLevel2());
                    }
                    //users.Add(new Users());
                    users[i + 1].ID = ((userID * 200) + 200) + i;
                    users[i + 1].Money = 50000;
                    users[i + 1].Name = "RandomUI";
                    users[i + 1].CompletedQuests = 0;
                }
            } 
            connection.Close();
            #endregion

            #region LoadingUserPortfolio
            queryString = "SELECT Portfolio.Stock, Portfolio.Count, Portfolio.User FROM Portfolio WHERE(((Portfolio.User) = " + userID.ToString() + "))";
            command = new OleDbCommand(queryString, connection);
            connection.Open();
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                items[reader.GetInt32(0) - 1].Holding = Math.Round((reader.GetInt32(1) / items[reader.GetInt32(0) - 1].SharesOutstanding) * 100, 2);
                items[reader.GetInt32(0) - 1].Count += reader.GetInt32(1);
                users[0].StockOwned[reader.GetInt32(0) - 1] = reader.GetInt32(1);
            } 
            #endregion

            #region LoadingAIUsersPortfolio
            for (int i = ((userID * 200) + 200); i < ((userID * 200) + 400); i++)
            {
                queryString = "SELECT Portfolio.Stock, Portfolio.Count, Portfolio.User FROM Portfolio WHERE(((Portfolio.User) = " + i.ToString() + "))";
                command = new OleDbCommand(queryString, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items[reader.GetInt32(0) - 1].Count += reader.GetInt32(1);
                    users[(i % 200) + 1].StockOwned[reader.GetInt32(0) - 1] = reader.GetInt32(1);
                }
            }
            connection.Close();
            #endregion

            #region LoadingEvents
            queryString = "SELECT * FROM Events";
            command = new OleDbCommand(queryString, connection);
            connection.Open();
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new Events() { ID = reader.GetInt32(0), Description = reader.GetString(1), Country = reader.GetString(2), Industry = reader.GetString(3), Stock = reader.GetInt32(4), Influence = reader.GetDouble(5) });
            }
            connection.Close();
            #endregion

        }

        private void ExecuteNonQuery(string queryString)
        {
            OleDbConnection conn = null;
            try
            {
                conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Filename + ";Persist Security Info=False;");
                conn.Open();

                OleDbCommand cmd = new OleDbCommand(queryString, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

    }
}
