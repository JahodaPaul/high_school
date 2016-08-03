using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace StockMarketSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool txtLoginMouseEnter;
        OleDbConnection connection = new OleDbConnection();
        string filename;
        int freeId = -1;
        Window1 window;
        public string currentDir;
        public LoadingData load;
        public int ID;

        public static string filenameS;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            txtLogin.Foreground = Brushes.Gray;
            filename = "Database_version_2.accdb";
            currentDir = Environment.CurrentDirectory;
            //finding a full path to current directory dynamically *** nachazení celé cesty k momentálnímu adresáři dynamicky
            while (!File.Exists(System.IO.Path.Combine(currentDir, filename)))
            {
                if (currentDir.Length <= 3)
                {
                    return;
                }
                currentDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDir, ".."));
            }
            filename = System.IO.Path.Combine(currentDir, filename);
            filenameS = filename;
        }

        private void txtLogin_MouseEnter(object sender, MouseEventArgs e)
        {
            txtLoginMouseEnter = true;
        }

        private void txtLogin_MouseLeave(object sender, MouseEventArgs e)
        {
            txtLoginMouseEnter = false;
        }

        private void txtLogin_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtLoginMouseEnter == true)
            {
                txtLogin.Text = "";
                txtLogin.Foreground = Brushes.Black;
            }
        }

        private void ExecuteNonQuery(string queryString)
        {
            OleDbConnection conn = null;
            try
            {
                conn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Persist Security Info=False;");
                conn.Open();

                OleDbCommand cmd = new OleDbCommand(queryString, conn);
                cmd.ExecuteNonQuery();
            }


            finally
            {
                if (conn != null) conn.Close();
            }
        }

        private void txtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            //[ENG]After enter is pressed, checks if the entered username is in the database or not -> will continue accordingly; if the username is not taken - it will find first not taken ID
            //[CZ]Pote co je zmacknuto tlacitko enter, zjisti jestli database obsahuje zadane jmeno a zaridi se podle toho; pokud neobsahuje, zjisti nejblizsi volny ID, ktere pri zapisovani do database bude uzivatel mit
            if (e.Key == Key.Enter)
            {
                connection.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Persist Security Info=False;";
                string queryString = "SELECT * FROM Users";
                OleDbCommand command = new OleDbCommand(queryString, connection);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();
                string userNames = "";
                bool login = true;

                List<int> IDs = new List<int>();
                while (reader.Read())
                {
                    IDs.Add(reader.GetInt32(0));
                    userNames += reader.GetString(1) + " ";
                }
                connection.Close();
                int counter = 0;
                while (freeId < 0)
                {
                    if (!IDs.Contains(counter))
                    {
                        freeId = counter;
                    }
                    else if (IDs.Contains(counter))
                    {
                        counter++;
                    }
                }
                int counter1 = 0;
                foreach (string item in userNames.Split(' '))
                {
                    if (item == txtLogin.Text)
                    {
                        ID = IDs[counter1];
                        Login();
                        login = true;
                        break;
                    }
                    else if (item != txtLogin.Text)
                    {
                        counter1++;
                        login = false;
                    }
                }
                if (login == false)
                {
                    ID = freeId;
                    //Create new user and login *** Vytvor noveho uzivatele v databazi a prihlas se 
                    queryString = "INSERT INTO Users VALUES (" + freeId + ",'" + txtLogin.Text + "','" + 50000 + "','" + 0 + "')";
                    command = new OleDbCommand(queryString, connection);
                    connection.Open();

                    ExecuteNonQuery(queryString);

                    connection.Close();
                    Login();
                }
                
            }
        }

        private void Login()
        {
            // login animation while loading data *** nacitani animace behem toho co se nacitaji data
            DoubleAnimation da = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(7)));
            DoubleAnimation fade = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(1)));
            RotateTransform rt = new RotateTransform();
            loadingImage.Visibility = Visibility.Visible;
            pointerImage.Visibility = Visibility.Visible;
            loadingImage.RenderTransform = rt;
            loadingImage.RenderTransformOrigin = new Point(0.5, 0.5);
            da.From = 0;
            da.To = 737;
            da.AccelerationRatio = 0.19;
            da.DecelerationRatio = 0.79;
            da.Completed += new EventHandler(Wait);
            rt.BeginAnimation(RotateTransform.AngleProperty, da);

            loadingImage.BeginAnimation(Image.OpacityProperty, fade);
            pointerImage.BeginAnimation(Image.OpacityProperty, fade);
            backGroundImageBrush.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(currentDir, "Images/worldBackgroundBlur.jpg")));

            load = new LoadingData(filename, ID);
            window = new Window1(this);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        }

        private void Wait(object sender, EventArgs e)
        {
            //half second waiting time before showing second "form" *** pulsekundova prodleva pred otevrenim dalsiho okna
            DoubleAnimation fade = new DoubleAnimation(0.9, new Duration(TimeSpan.FromMilliseconds(500)));
            fade.Completed += new EventHandler(ShowWindow);
            loadingImage.BeginAnimation(Image.OpacityProperty, fade);
            pointerImage.BeginAnimation(Image.OpacityProperty, fade);
        }

        private void ShowWindow(object sender, EventArgs e)
        {
            this.Close();
            window.Show();
        }
    }
}
