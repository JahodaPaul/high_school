using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.Charts;
using System.Data.OleDb;

namespace StockMarketSimulation
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        int userID;
        Grid stocksGrid;
        Image[] arrayImage;
        BitmapImage[] newsImages;
        List<int> eventsIDArchive = new List<int>();
        List<Events> eventsArchive = new List<Events>();
        int QuestsCompleted = 0;
        int selectedImage = 0;
        string[] Quests = new string[16];
        string[] QuestsCzech = new string[16];
        string[] Questions = new string[10];
        string[] QuestionsCzech = new string[10];
        int questionsAnswered = 0;
        string title;
        string titleCzech;
        listViewGridView listView;
        ObservableDataSource<Point> source1 = null;
        int currentStockID;
        List<Grid> grids;
        List<ListView> listViews;
        private List<Order> listOfOrders;
        private Stocks obj;
        private int selectedIndex;
        private List<double> priceList;
        private List<DateTime> dateList;
        private int answQtoComplete = 0;
        private int _myUpdate;

        public int MyUpdate
        {
            get { return _myUpdate; }
            set
            {
                _myUpdate = value;
                try
                {
                    if (_myUpdate == 1)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {

                            foreach (var item in listView.users)
                            {
                                //item.Decision(listView.items, this);
                                item.PlaceOrders(this);
                            }
                            for (int i = 0; i < listView.items.Count; i++)
                            {
                                if (listView.users[0].StockOwned[i] == 0)
                                {
                                    listView.items[i].Value = 0;
                                }
                                else if (listView.users[0].StockOwned[i] != 0)
                                {
                                    listView.items[i].Value = (int)(listView.items[i].Price * listView.users[0].StockOwned[i]);
                                }
                                if (listView.items[0].priceHistory.Count >= 2)
                                {
                                    if (listView.items[i].priceHistory[listView.items[i].priceHistory.Count - 1] > listView.items[i].priceHistory[0])
                                    {
                                        listView.items[i].Change = ((listView.items[i].priceHistory[listView.items[i].priceHistory.Count - 1] / listView.items[i].priceHistory[0]) * 100 - 100);
                                        listView.items[i].Change = Math.Round(listView.items[i].Change);
                                        listView.items[i].ChangePositive = 2;
                                    }
                                    else if (listView.items[i].priceHistory[listView.items[i].priceHistory.Count - 1] < listView.items[i].priceHistory[0])
                                    {
                                        listView.items[i].Change = (100 - ((listView.items[i].priceHistory[listView.items[i].priceHistory.Count - 1] / listView.items[i].priceHistory[0]) * 100));
                                        listView.items[i].Change = Math.Round(listView.items[i].Change);
                                        listView.items[i].ChangePositive = 1;
                                    }
                                    else
                                    {
                                        listView.items[i].Change = 0;
                                        listView.items[i].ChangePositive = 0;
                                    }
                                }
                            }

                            txtPrice.Text = (Math.Round(listView.items[selectedIndex].Price, 2)).ToString();
                            plotter.Children.RemoveAll(typeof(LineGraph));
                            var datesDataSource = new EnumerableDataSource<DateTime>(dateList);
                            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));
                            var priceDataSource = new EnumerableDataSource<double>(priceList);
                            priceDataSource.SetYMapping(y => y);
                            CompositeDataSource compositeDataSource1 = new CompositeDataSource(datesDataSource, priceDataSource);
                            plotter.AddLineGraph(compositeDataSource1);
                            plotter.Viewport.FitToView();
                            _myUpdate = 0;
                            if (priceList.Count % 5 == 0 && priceList.Count != 0)
                            {
                                foreach (var item in listView.users)
                                {
                                    for (int i = 0; i < item.StockOwned.Length; i++)
                                    {
                                        if (item.StockOwned[i] > 0)
                                        {
                                            item.Money += item.StockOwned[i] * (listView.items[i].Price * (listView.items[i].DividendYield / 400));
                                        }
                                    }
                                }
                            }
                            if (priceList.Count % 2 == 0 && priceList.Count != 0)
                            {
                                if (eventsIDArchive.Count < listView.events.Count)
                                {
                                    GetEvent();
                                    SetPopularity();
                                }
                            }
                            if (listView.users[0].CompletedQuests == 3 && listView.users[0].Money >= 55000)
                            {
                                listView.users[0].Money += Convert.ToInt32(txtReward.Text);
                                txtMoney.Text = ((int)listView.users[0].Money).ToString();
                                listView.users[0].CompletedQuests += 1;
                                selectedImage += 1;
                                UpdateQuests();
                            }
                        }));
                    }
                    if (_myUpdate == 2)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {

                            foreach (var item in listView.users)
                            {
                                item.Decision(listView.items);
                            }
                        }));
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        Thread matchingStocks;

        public Window1(MainWindow window)
        {
            InitializeComponent();
            userID = window.ID;
            source1 = new ObservableDataSource<Point>();
            // Set identity mapping of point in collection to point on plot
            source1.SetXYMapping(p => p);
           
            // Add all three graphs. Colors are not specified and chosen random
            plotter.AddLineGraph(source1, 2, "Data row 1");

            // Start computation process in second thread
            Thread simThread = new Thread(new ThreadStart(AnotherThread));
            simThread.IsBackground = true;
            //simThread.Start();
            stocksGrid = new Grid();
            stocksGrid.Width = 400;
            stocksGrid.HorizontalAlignment = HorizontalAlignment.Left;
            stocksGrid.VerticalAlignment = VerticalAlignment.Top;
            stocksGrid.ShowGridLines = true;
            stocksGrid.Background = new SolidColorBrush(Colors.LightSteelBlue);
            //mediaElement.Source = new Uri(System.IO.Path.Combine(window.currentDir, "Images/cinemagraph_Whiskey.gif"));
            listView = new listViewGridView(this,window.load);
            
            txtMoney.Text = ((int)listView.users[0].Money).ToString();
            arrayImage = new Image[16] { image3, image4, image5, image6, image7, image8, image9, image10, image11, image12, image13, image14, image15, image16, image17, image18 };

            newsImages = new BitmapImage[] { new BitmapImage(new Uri("/Images/news/01.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/02.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/03.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/04.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/05.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/06.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/07.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/08.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/09.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/10.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/11.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/12.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/13.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/14.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/15.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/16.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/17.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/18.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/19.jpg", UriKind.Relative)),
                                             new BitmapImage(new Uri("/Images/news/20.jpg", UriKind.Relative)), };

            listBox.ItemsSource = eventsArchive;

            grids = new List<Grid>() { gridCurrentStock, gridCurrentQuest, gridQuests, gridPortfolio, gridNews};
            listViews = new List<ListView> {listViewPortfolio, listViewStocks,listViewRanking };
            listOfOrders = new List<Order>();
            //[ENG]Dynamical assignment of image sources to the images, depending on quest compeletation; dynamical asignment of events
            //[CZ]dynamicke prirazeni obrazku, podle toho jestli je Quest splnen nebo ne; dynamicke prirazeni eventů..

            UpdateQuests();
            if (listView.users[0].CompletedQuests == 2)
            {
                questionsAnswered = 3;
            }
            else if (listView.users[0].CompletedQuests == 3 || listView.users[0].CompletedQuests == 4 || listView.users[0].CompletedQuests == 5)
            {
                questionsAnswered = 5;
            }
            else if (listView.users[0].CompletedQuests == 6)
            {
                questionsAnswered = 7;
            }
            else if (listView.users[0].CompletedQuests == 7)
            {
                questionsAnswered = 9;
            }


            Quests[0] = "Place a limit order to buy 50 stocks of Ford;500;1;Go to list of stocks by clicking on \"Stocks\" and then doubleclicking the row containing Ford stocks and simply select button \"Limit Order\" and after selecting 50 shares or more using slider, simply click on button \"Buy\" ";
            QuestsCzech[0] = "Umísti limitní příkaz na nákup 50 akcií Fordu;500;1;Přejďete na seznam akcií kliknutím na tlačítko \"Stocks\" a potom dvojklikem klikněte na řádek akcií Ford. Po zmáčknutí tlačítka \"Limit Order\" vyberte posuvníkem více než 50 akcií a zmáčknete tlačítko \"Buy\" ";
            Quests[1] = "Pick correct answer;300;1;You receive 300 for each correctly answered question and lose 150 for each incorrect answer, pick the right answer by clicking at the button with right answer;3;BTN";
            QuestsCzech[1] = "Vyber správné odpovědi;300;1;Za každou správně zodpovězenou otázku získáte 300 a za každou špatně zodpovězenou se vám 150 strhne, výběr správné otázky probíhá kliknutím na tlačítko se správnou odpovědí;3;BTN";
            Quests[2] = "Pick factually wrong sentences;300;2;You receive 300 for each correctly picked false statement and lose 150 for each incorrectly pick false statement;5;BTN";
            QuestsCzech[2] = "Vyber fakticky nesprávné věty;300;2;Za každou správně vybranou špatnou větu získáte 300 a za každou špatně zodpovězenou se vám 150 strhne;5;BTN";
            Quests[3] = "Have 55 000 dollars or more;500;3; ";
            QuestsCzech[3] = "Měj 55 000 nebo více dolarů;500;3; ";
            Quests[4] = "Place a limit order to sell 100 or more shares of any company;1000;1;Go to list of stocks by clicking on \"Stocks\" and then doubleclicking the row containing stocks you want to sell and simply select button \"Limit Order\" and after selecting 100 shares or more using slider, simply click on button \"Sell\" ";
            QuestsCzech[4] = "Umísti limitní příkaz na prodej 100 nebo více akcií libovolné společnosti;1000;1;Přejďete na seznam akcií kliknutím na tlačítko \"Stocks\" a potom dvojklikem klikněte na řádek akcií společnosti které chcete koupit. Po zmáčknutí tlačítka \"Limit Order\" vyberte posuvníkem více než 100 akcií a zmáčknete tlačítko \"Sell\"";
            Quests[5] = "Pick correct answer;400;3;You receive 400 for each correctly answered question and lose 200 for each incorrect answer, pick the right answer by clicking at the button with right answer;7;BTN";
            QuestsCzech[5] = "Vyber správné odpovědi;400;3;Za každou správně zodpovězenou otázku získáte 400 a za každou špatně zodpovězenou se vám 200 strhne, výběr správné otázky probíhá kliknutím na tlačítko se správnou odpovědí;7;BTN";
            Quests[6] = "Pick factually wrong sentences;400;4;You receive 400 for each correctly picked false statement and lose 200 for each incorrectly pick false statement;9;BTN";
            QuestsCzech[6] = "Vyber fakticky nesprávné věty;400;4;Za každou správně vybranou špatnou větu získáte 400 a za každou špatně zodpovězenou se vám 200 strhne;9;BTN";
            Quests[7] = "Own 100 percent of any 2 companies;2000;6; ";
            QuestsCzech[7] = "Vlastni 100 procent libovolných 2 společností;2000;6; ";



            Questions[0] = "What is a dividend?;Payment made by a corporation to its shareholders, usually as a distribution of profits;Payment made by a corporation to its shareholders as assets;Financial statement that summarizes a company's assets;1";
            QuestionsCzech[0] = "Co to je dividenda?;Peněžité plnění akciových společností vyplácené akcionářům, obvykle na základě zisku společnosti;Peněžité plnění akciových společností vyplácené akcionářům na základě aktiv společnosti;Finanční prohlášení shrnující aktiva společnosti;1";
            Questions[1] = "Choose country with the highest GDP per capita(year 2015);Russia;Norway;Germany;2";
            QuestionsCzech[1] = "Vyberte zemi s nejvyším hrubým domacím produktem na obyvatele(rok 2015);Rusko;Norsko;Německo;2";
            Questions[2] = "Choose country with the highest average wage;United States;United Kingdom;Italy;1";
            QuestionsCzech[2] = "Vyberte zemi s nejvyšší průměrnou mzdou;USA;Británie;Itálie;1";
            Questions[3] = " ;Macroeconomics deals with broad economies and larger things such as interest rates, Gross Domestic Product (GDP) ect.;Microeconomics deals with customer behavior, incentives, pricing, margins, etc.;Macroeconomics deals with customer behavior, incentives, pricing, margins, etc.;3";
            QuestionsCzech[3] = " ;Makroekonomie se zabývá ekonomickým systémem jako celku, zabývá se například urokovými sazbavi,Hrubým domácím produktem(HDP) atd.;Mikroekonomie se zabývá chováním zákazníku, stanovením cen,maržema atd.;Makroekonomie se zabývá chováním zákazníku, stanovením cen,maržema atd.;3";
            Questions[4] = " ;If supply of something increases its price decreases;If demand of something decreases its price increases; If supply decreases price increases;2";
            QuestionsCzech[4] = " ;Pokud se zvýší nabídka nečeho tak cena něčeho jde dolů;Pokud se sníží poptávka po něčem, tak se cena zvýší; Pokud se sníží nabídka něčeho tak jde cena nečeho nahorů;2";
            Questions[5] = "Choose a management style, where the manager makes decisions unilaterally, and without much regard for subordinates;Autocratic;Democratic;Liberal;1";
            QuestionsCzech[5] = "Vyberte styl řízení podniku ve kterém manažer preferuje využívání pravomoci bez konzultace s podřízenými;Autokratický;Demokratický;Liberální;1";
            Questions[6] = "Pick the best country for doing business;USA;Hong Kong;Singapore;3";
            QuestionsCzech[6] = "Vyberte nejlepší zemi pro podnikání;USA;Hong Kong;Singapore;3";
            Questions[7] = " ;United States has higher wealth inequality than Czech Republic;Czech Republic has higher wealth inequality than Russia;Russia has higher wealth inequality than Germany;2";
            QuestionsCzech[7] = " ;USA má větší nerovnost mezi bohatými a chudými než Česká Republika;Česká Republika má vyšší nerovnost mezi bohatými a chudými než Rusko;Rusko má vetší nerovnost mezi bohatými a chudými než Německo;2";
            Questions[8] = " ;United States has lower tax on goods than Czech Republic;South Korea has lower tax on goods than Czech Republic;Denmark has lower tax on goods than Czech Republic;3";
            QuestionsCzech[8] = " ;USA má nižší DPH na spotřební zboží nez Česká Republika;Jižní Korea má nižší DPH na spotřební zboží nez Česká Repulika;Dánsko má nižší DPH na spotřební zboží než Česká Republika;3";

            title = "The simulator emulates real stock markets.\nShare prices are changing a lot faster in this simulator,which enables the user to try out how the stock market works.\n\nThe simulator doesn't download data from real stock market, but prices are rather affected by artificial inteligence(other traders).\n\nThe simulator provides the user with the opportunity to learn basic economical principles through system of quests.";
            titleCzech = "Tento simulátoru napodobuje skutečné burzy.\nCeny akcií se v tomto simulátoru mění rychleji, tudíž je časově nenáročné vyzkoušet si fungování burzy.\n\nSimulátor nestahuje data ze skutečné burzy, ale ceny akcíí jsou ovlivněny umělou inteligencí(ostatními obchodníky).\n\nSimulátor umožňuje uživateli naučit se základní ekonomické principy a termíny skrze systém úkolů";

            TitleText.Text = title;

            for (int i = 8; i < 16; i++)
            {
                Quests[i] = "";
                QuestsCzech[i] = "";
            }
            for (int i = 0; i < 93; i++)
            {
                placeOrder((int)listView.items[i].SharesOutstanding, i, "LimitOrder", (int)listView.items[i].Price, "Sell", -1);
            }

            matchingStocks = new Thread(new ThreadStart(MatchingEngine));
            matchingStocks.IsBackground = true;
            matchingStocks.Start();
        }

        private void UpdateQuests()
        {
            QuestsCompleted = listView.users[0].CompletedQuests;
            for (int i = 0; i < 16; i++)
            {
                if (i < QuestsCompleted)
                {

                }
                else if (i == QuestsCompleted)
                {
                    if (arrayImage[i].Source.ToString().EndsWith("Uncompleted.png"))
                    {
                        string path = arrayImage[i].Source.ToString();
                        int indexOfLastChar = path.LastIndexOf('g');
                        path = path.Substring(0, indexOfLastChar - 14);
                        path += ".png";
                        arrayImage[i].Source = null;
                        arrayImage[i].Source = new BitmapImage(new Uri(path));
                    }
                    selectedImage = i;
                    arrayImage[selectedImage].MouseEnter += biggerImage;
                    arrayImage[selectedImage].MouseLeave += smallerImage;
                    arrayImage[selectedImage].MouseDown += ShowQuest;
                    if (selectedImage != 0)
                    {
                        arrayImage[selectedImage - 1].MouseEnter -= biggerImage;
                        arrayImage[selectedImage - 1].MouseLeave -= smallerImage;
                        arrayImage[selectedImage - 1].MouseDown -= ShowQuest;
                    }
                }
                else
                {
                    if (!arrayImage[i].Source.ToString().EndsWith("Uncompleted.png"))
                    {
                        string path = arrayImage[i].Source.ToString();
                        string[] array = path.Split('.');
                        path = array[0];
                        path += "Uncompleted.png";
                        arrayImage[i].Source = new BitmapImage(new Uri(path)); 
                    }
                }
            }
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {

        }

        private void btnStocks_Click(object sender, RoutedEventArgs e)
        {
            listViewStocks.ItemsSource = null;
            listViewStocks.ItemsSource = listView.LoadStocks();
            visibilityMethod(listViewStocks);

        }

        private void btnRanking_Click(object sender, RoutedEventArgs e)
        {
            listViewRanking.ItemsSource = null;
            listViewRanking.ItemsSource = listView.LoadRankings();
            visibilityMethod(listViewRanking);
        }

        private void btnPortfolio_Click(object sender, RoutedEventArgs e)
        {
            txtMoney.Text = ((int)listView.users[0].Money).ToString();
            listViewPortfolio.ItemsSource = null;
            listViewPortfolio.ItemsSource = listView.LoadPortfolio();
            visibilityMethod(gridPortfolio, listViewPortfolio);
            image19.Source = arrayImage[selectedImage].Source;
        }

        private void btnNews_Click(object sender, RoutedEventArgs e)
        {
            visibilityMethod(gridNews);
            DoubleAnimation da = new DoubleAnimation();
            da.From = -textBlockNews.ActualWidth;
            da.To = canvasNews.ActualWidth - textBlockNews2.ActualWidth;
            da.RepeatBehavior = RepeatBehavior.Forever;
            //speed of movement dependent on its width *** rychlost textu závislá na šířce onoho textu
            int seconds = (textBlockNews.Text.Length + 8)/ 8;
            int min = 0;

            while (seconds > 60)
            {
                seconds -= 60;
                min++;
            }

            da.Duration = new Duration(TimeSpan.Parse("0:" + min.ToString() + ":" + seconds.ToString()));
            textBlockNews.BeginAnimation(Canvas.RightProperty, da);
        }

        private void btnQuests_Click(object sender, RoutedEventArgs e)
        {
            visibilityMethod(gridQuests);
        }

        private void visibilityMethod(params object[] items)
        {
            gridStartScreen.Visibility = Visibility.Hidden;
            foreach (var item in grids)
            {
                if (items.Contains(item))
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Hidden;
                }
            }
            foreach (var item in listViews)
            {
                if (items.Contains(item))
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Hidden;
                }
            }
        }

        private void buttonBuy_Click(object sender, RoutedEventArgs e)
        {
            if (slider.Visibility == Visibility.Hidden)
            {
                placeOrder((int)StockCountSliderBuy.Value, listViewStocks.SelectedIndex, "MarketOrder", 0, "Buy",userID);
                txtPurchase.Text = "You have made a Market Order to buy " + ((int)StockCountSliderBuy.Value).ToString() + " shares of " + obj.Name.ToString() + ".";
            }
            else if (slider.Visibility == Visibility.Visible)
            {
                if (listView.users[0].CompletedQuests == 0 && listViewStocks.SelectedIndex == 9 && (int)StockCountSliderBuy.Value >= 50 )
                {
                    listView.users[0].Money += 500;
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                    listView.users[0].CompletedQuests += 1;
                    selectedImage += 1;
                    UpdateQuests();
                }
                placeOrder((int)StockCountSliderBuy.Value, listViewStocks.SelectedIndex, "LimitOrder",(int)(Convert.ToDouble(txtLimitOrder.Text)), "Buy",userID);
                txtPurchase.Text = "You have made a Limit Order to buy " + ((int)StockCountSliderBuy.Value).ToString() + " shares of " + obj.Name.ToString() + ".";
            }

            listViewStocks.ItemsSource = null;
            listViewStocks.ItemsSource = listView.LoadStocks();
            listViewPortfolio.ItemsSource = listView.LoadPortfolio();
            txtMoney.Text = ((int)listView.users[0].Money).ToString();
            
            if (listView.users[0].Money > ((listView.items[currentStockID].Price * listView.items[currentStockID].SharesOutstanding) - (listView.items[currentStockID].Count * listView.items[currentStockID].Price)))
            {
                StockCountSliderBuy.Maximum = listView.items[currentStockID].SharesOutstanding;
            }
            else
            {
                int canAfford = (int)(listView.users[0].Money / listView.items[currentStockID].Price);
                StockCountSliderBuy.Maximum = canAfford;
            }
            StockCountSliderBuy.Value = 0;
            StockCountSliderSell.Value = 0;
        }

        private void buttonSell_Click(object sender, RoutedEventArgs e)
        {
            if (slider.Visibility == Visibility.Hidden)
            {
                placeOrder((int)StockCountSliderSell.Value, listViewStocks.SelectedIndex, "MarketOrder", 0, "Sell",userID);
                txtPurchase.Text = "You have made a Market Order to sell " + ((int)StockCountSliderSell.Value).ToString() + " shares of " + obj.Name.ToString() + ".";
            }
            else if (slider.Visibility == Visibility.Visible)
            {
                if (listView.users[0].CompletedQuests == 4 && (int)StockCountSliderSell.Value >= 100)
                {
                    listView.users[0].Money += Convert.ToInt32(txtReward.Text);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                    listView.users[0].CompletedQuests += 1;
                    selectedImage += 1;
                    UpdateQuests();
                }
                placeOrder((int)StockCountSliderSell.Value, listViewStocks.SelectedIndex, "LimitOrder", (int)(Convert.ToDouble(txtLimitOrder.Text)), "Sell",userID);
                txtPurchase.Text = "You have made a Limit Order to sell " + ((int)StockCountSliderSell.Value).ToString() + " shares of " + obj.Name.ToString() + ".";
            }

            listViewStocks.ItemsSource = null;
            listViewStocks.ItemsSource = listView.LoadStocks();
            listViewPortfolio.ItemsSource = listView.LoadPortfolio();
            txtMoney.Text = ((int)listView.users[0].Money).ToString();
            StockCountSliderSell.Maximum = listView.users[0].StockOwned[currentStockID];
            StockCountSliderBuy.Value = 0;
            StockCountSliderSell.Value = 0;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void biggerImage(object sender,MouseEventArgs e)
        {
            arrayImage[selectedImage].Height += 7;
        }

        private void smallerImage(object sender, MouseEventArgs e)
        {
            arrayImage[selectedImage].Height -= 7;
        }

        private void ShowQuest(object sender, MouseEventArgs e)
        {
            //Dynamical assignment of quest *** dynamické přiřazení questu
            visibilityMethod(gridCurrentQuest);
            btnLanguage.Content = "CZ";
            QuestsText(Quests,Questions);
        }

        private void btnTitleLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (btnTitleLanguage.Content.ToString() == "CZ")
            {
                btnTitleLanguage.Content = "ENG";
                TitleText.Text = titleCzech;
            }
            else
            {
                btnTitleLanguage.Content = "CZ";
                TitleText.Text = title;
            }
        }

        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (btnLanguage.Content.ToString() == "CZ")
            {
                btnLanguage.Content = "ENG";
                QuestsText(QuestsCzech,QuestionsCzech);
            }
            else
            {
                btnLanguage.Content = "CZ";
                QuestsText(Quests,Questions);
            }
        }

        private void QuestsText(string[] StringsForQuest,string[] Questions)
        {
            string[] array = StringsForQuest[selectedImage].Split(';');
            txtTask.Text = array[0];
            txtReward.Text = array[1];
            progressBarDifficulty.Value = Convert.ToInt32(array[2]);
            txtHelp.Text = array[3];
            if (StringsForQuest[selectedImage].EndsWith("BTN"))
            {
                answQtoComplete = Convert.ToInt32(array[4]);
                menuForButtons.Visibility = Visibility.Visible;
                string[] array2 = Questions[questionsAnswered].Split(';');
                txtQuestion.Text = array2[0];
                btn1.Content = array2[1];
                btn2.Content = array2[2];
                btn3.Content = array2[3];
            }
            else
            {
                menuForButtons.Visibility = Visibility.Hidden;
                txtQuestion.Text = "";
            }
        }

        public bool colourChanged = false;

        private void ColourCorrect()
        {
            DoubleAnimation fade = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(500)));
            fade.Completed += new EventHandler(ColourChanged);
            BeginAnimation(Image.OpacityProperty, fade);
            if (Questions[questionsAnswered].EndsWith("1"))
            {
                btn1.Background = Brushes.Green;
            }
            else if (Questions[questionsAnswered].EndsWith("2"))
            {
                btn2.Background = Brushes.Green;
            }
            else if (Questions[questionsAnswered].EndsWith("3"))
            {
                btn3.Background = Brushes.Green;
            }
        }

        private void ColourChanged(object sender, EventArgs e)
        {
            colourChanged = true;
            if (Questions[questionsAnswered].EndsWith("1"))
            {
                btn1.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                btn1_Click(sender, null);
            }
            else if (Questions[questionsAnswered].EndsWith("2"))
            {
                btn2.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                btn2_Click(sender, null);
            }
            else if (Questions[questionsAnswered].EndsWith("3"))
            {
                btn3.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                btn3_Click(sender, null);
            }
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (!colourChanged)
            {
                ColourCorrect();
            }
            else if (colourChanged)
            {
                if (Questions[questionsAnswered].EndsWith("1"))
                {
                    listView.users[0].Money += Convert.ToInt32(txtReward.Text);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                else
                {
                    listView.users[0].Money -= (Convert.ToInt32(txtReward.Text) / 2);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                questionsAnswered += 1;
                if (questionsAnswered == answQtoComplete)
                {
                    listView.users[0].CompletedQuests += 1;
                    selectedImage += 1;
                    UpdateQuests();
                    visibilityMethod(gridQuests);
                }
                if (btnLanguage.Content.ToString() == "CZ" && questionsAnswered != answQtoComplete)
                {
                    QuestsText(Quests, Questions);
                }
                else if (questionsAnswered != answQtoComplete)
                {
                    QuestsText(QuestsCzech, QuestionsCzech);
                }
                colourChanged = false;
            }
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            if (!colourChanged)
            {
                ColourCorrect();
            }
            else if (colourChanged)
            {
                if (Questions[questionsAnswered].EndsWith("2"))
                {
                    listView.users[0].Money += Convert.ToInt32(txtReward.Text);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                else
                {
                    listView.users[0].Money -= (Convert.ToInt32(txtReward.Text) / 2);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                questionsAnswered += 1;
                if (questionsAnswered == answQtoComplete)
                {
                    listView.users[0].CompletedQuests += 1;
                    selectedImage += 1;
                    UpdateQuests();
                    visibilityMethod(gridQuests);
                }
                if (btnLanguage.Content.ToString() == "CZ" && questionsAnswered != answQtoComplete)
                {
                    QuestsText(Quests, Questions);
                }
                else if (questionsAnswered != answQtoComplete)
                {
                    QuestsText(QuestsCzech, QuestionsCzech);
                }
                colourChanged = false; 
            }
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            if (!colourChanged)
            {
                ColourCorrect();
            }
            else if (colourChanged)
            {
                if (Questions[questionsAnswered].EndsWith("3"))
                {
                    listView.users[0].Money += Convert.ToInt32(txtReward.Text);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                else
                {
                    listView.users[0].Money -= (Convert.ToInt32(txtReward.Text) / 2);
                    txtMoney.Text = ((int)listView.users[0].Money).ToString();
                }
                questionsAnswered += 1;
                if (questionsAnswered == answQtoComplete)
                {
                    listView.users[0].CompletedQuests += 1;
                    selectedImage += 1;
                    UpdateQuests();
                    visibilityMethod(gridQuests);
                }
                if (btnLanguage.Content.ToString() == "CZ" && questionsAnswered != answQtoComplete)
                {
                    QuestsText(Quests, Questions);
                }
                else if (questionsAnswered != answQtoComplete)
                {
                    QuestsText(QuestsCzech, QuestionsCzech);
                }
                colourChanged = false; 
            }
        }

        private void listViewStocks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dateList = new List<DateTime>();
            priceList = new List<double>();
            txtPurchase.Text = " ";

            plotter.Children.RemoveAll(typeof(LineGraph));
            //Gets which share did you select *** ziska kterou akcii jsi si vybral
            selectedIndex = listViewStocks.SelectedIndex;
            if (selectedIndex >= 0)
            {
                
                obj = listView.items[selectedIndex];
                txtCountry.Text = obj.Country;
                txtDividendYield.Text = obj.DividendYield.ToString();
                txtIndustry.Text = obj.Industry;
                txtName.Text = obj.Name;
                txtPriceEarnings.Text = obj.PriceEarnings.ToString();
                txtSharesOustanding.Text = obj.SharesOutstanding.ToString();
                txtVolume.Text = obj.Volume.ToString();
                txtPrice.Text = Math.Round(obj.Price, 2).ToString();
                currentStockID = obj.ID;
                double tmp = obj.Price * 0.2;
                slider.Minimum = obj.Price - tmp;
                slider.Maximum = obj.Price + tmp;
                slider.Value = obj.Price;
                txtLimitOrder.Text = slider.Value.ToString();
                
                if (obj.Count > 0)
                {
                    if (listView.users[0].Money > ((obj.Price * obj.SharesOutstanding) - (obj.Count * obj.Price)))
                    {
                        StockCountSliderBuy.Maximum = obj.SharesOutstanding - obj.Count;
                    }
                    else
                    {
                        int canAfford = (int)(listView.users[0].Money / obj.Price);
                        StockCountSliderBuy.Maximum = canAfford;
                    }
                }
                else
                {
                    if (listView.users[0].Money > (obj.Price * obj.SharesOutstanding))
                    {
                        StockCountSliderBuy.Maximum = obj.SharesOutstanding;
                    }
                    else
                    {
                        int canAfford = (int)(listView.users[0].Money / obj.Price);
                        StockCountSliderBuy.Maximum = canAfford;
                    }
                }
                StockCountSliderSell.Maximum = listView.users[0].StockOwned[selectedIndex];

                dateList = obj.dateHistory;
                priceList = obj.priceHistory;
            }
            
            visibilityMethod(gridCurrentStock);

            var datesDataSource = new EnumerableDataSource<DateTime>(dateList);
            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));

            var priceDataSource = new EnumerableDataSource<double>(priceList);
            priceDataSource.SetYMapping(y => y);
           
            CompositeDataSource compositeDataSource1 = new CompositeDataSource(datesDataSource, priceDataSource);

            plotter.AddLineGraph(compositeDataSource1);

            plotter.Viewport.FitToView();

        }

        private void AnotherThread()
        {
        }

        private void StockCountSliderBuy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StockCountBuyTextBlock.Text = ((int)StockCountSliderBuy.Value).ToString();
        }

        private void StockCountSliderSell_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StockCountSellTextBlock.Text = ((int)StockCountSliderSell.Value).ToString();
        }

        private void limitOrderbtn_Click(object sender, RoutedEventArgs e)
        {
            slider.Visibility = Visibility.Visible;
            txtLimitOrder.Visibility = Visibility.Visible;
            limitOrderbtn.Background = new SolidColorBrush(Color.FromRgb(20, 149, 32));
            MarketOrderbtn.Background = new SolidColorBrush(Color.FromRgb(191, 191, 191));
            slider.Focus();
        }

        private void MarketOrderbtn_Click(object sender, RoutedEventArgs e)
        {
            slider.Visibility = Visibility.Hidden;
            txtLimitOrder.Visibility = Visibility.Hidden;
            limitOrderbtn.Background = new SolidColorBrush(Color.FromRgb(191, 191, 191));
            MarketOrderbtn.Background = new SolidColorBrush(Color.FromRgb(20, 149, 32));
            StockCountSliderBuy.Focus();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtLimitOrder.Text = Math.Round(slider.Value, 2).ToString();
        }

        public void placeOrder(int numberOfShares, int index, string type,int stopLoss,string buyOrSell,int IDUser)
        {
            if (buyOrSell == "Sell")
            {
                listView.items[index].Count -= numberOfShares;
            }
            listOfOrders.Add(new Order(numberOfShares, index, type, stopLoss, buyOrSell,IDUser));
        }

        private static void Swap(List<Order> array, int left, int right)
        {
            Order tmp = array[right];
            array[right] = array[left];
            array[left] = tmp;
        }

        private static void Quicksort(List<Order> array, int left, int right)
        {
            if (left < right)
            {
                int boundary = left;
                for (int i = left + 1; i < right; i++)
                {
                    if (array[i].Index < array[left].Index)
                    {
                        Swap(array, i, ++boundary);
                    }
                }
                Swap(array, left, boundary);
                Quicksort(array, left, boundary);
                Quicksort(array, boundary + 1, right);
            }
        }

        private void MatchingEngine()
        {
            // do nekonecna opakujici se matchingEngine -> spojuje spolu a nakupujici a prodavajici a vytvari cenu akcie na zaklade nabidky a poptavky
            while (true)
            {
                MyUpdate = 2;// pusti se hlavni thread, kde se delaji obchodnici rozhoduji co v budou nakoupi a prodaji
                DateTime before = DateTime.Now;
                List<Order> temporaryBuy = new List<Order>();
                List<Order> temporarySell = new List<Order>();
                Quicksort(listOfOrders, 0, listOfOrders.Count);
                int counter = 0;
                int counter2 = 0;
                double dividedNumber = 0;
                int dividedBy = 0;
                for (int i = 0; i < 93; i++)
                {
                    while (counter < listOfOrders.Count && listOfOrders[counter].StopLoss != -1 && listOfOrders[counter].Index == i)
                    {
                        if (listOfOrders[counter].BuyOrSell == "Buy")
                        {
                            temporaryBuy.Add(new Order(listOfOrders[counter].Amount, listOfOrders[counter].Index, listOfOrders[counter].Type, listOfOrders[counter].StopLoss, listOfOrders[counter].BuyOrSell, listOfOrders[counter].UserId));
                        }
                        else if (listOfOrders[counter].BuyOrSell == "Sell")
                        {
                            temporarySell.Add(new Order(listOfOrders[counter].Amount, listOfOrders[counter].Index, listOfOrders[counter].Type, listOfOrders[counter].StopLoss, listOfOrders[counter].BuyOrSell, listOfOrders[counter].UserId));
                        }
                        counter++;
                    }
                    if (temporaryBuy.Count > 0 && temporarySell.Count > 0)
                    {
                        if (temporaryBuy.Count > 1)
                        {
                            for (int k = 0; k < temporaryBuy.Count - 1; k++)
                            {
                                int j = k + 1;
                                Order temporary = temporaryBuy[j];
                                while (j > 0 && temporary.StopLoss < temporaryBuy[j - 1].StopLoss)
                                {
                                    temporaryBuy[j] = temporaryBuy[j - 1];
                                    j--;
                                }
                                temporaryBuy[j] = temporary;
                            }
                        }
                        if (temporarySell.Count > 1)
                        {
                            for (int k = 0; k < temporarySell.Count - 1; k++)
                            {
                                int j = k + 1;
                                Order temporary = temporarySell[j];
                                while (j > 0 && temporary.StopLoss < temporarySell[j - 1].StopLoss)
                                {
                                    temporarySell[j] = temporarySell[j - 1];
                                    j--;
                                }
                                temporarySell[j] = temporary;
                            }
                        }
                        if (temporaryBuy.Count > 0 && temporarySell.Count > 0)
                        {
                            for (int j = 0; j < temporaryBuy.Count; j++)
                            {
                                if (temporaryBuy[j].StopLoss == 0)
                                {
                                    if (temporaryBuy[temporaryBuy.Count - 1].StopLoss == 0)
                                    {
                                        temporaryBuy[j].StopLoss = listView.items[temporaryBuy[j].Index].Price;
                                    }
                                    if (temporaryBuy[temporaryBuy.Count - 1].StopLoss >= temporarySell[0].StopLoss && temporarySell[0].StopLoss != 0)
                                    {
                                        temporaryBuy[j].StopLoss = temporaryBuy[temporaryBuy.Count - 1].StopLoss;
                                    }
                                    else if (temporaryBuy[temporaryBuy.Count - 1].StopLoss < temporarySell[0].StopLoss && temporarySell[0].StopLoss != 0)
                                    {
                                        temporaryBuy[j].StopLoss = temporarySell[0].StopLoss;
                                    }
                                    int counter3 = 0;
                                    while (counter3 < temporarySell.Count && temporarySell[counter3].StopLoss == 0)
                                    {
                                        temporaryBuy[j].StopLoss = temporaryBuy[temporaryBuy.Count - 1].StopLoss;
                                        temporarySell[counter3].StopLoss = listView.items[temporarySell[counter3].Index].Price;
                                        counter3++;
                                    }
                                }
                            }
                            if (temporaryBuy.Count > 1)
                            {
                                for (int k = 0; k < temporaryBuy.Count - 1; k++)
                                {
                                    int j = k + 1;
                                    Order temporary = temporaryBuy[j];
                                    while (j > 0 && temporary.StopLoss < temporaryBuy[j - 1].StopLoss)
                                    {
                                        temporaryBuy[j] = temporaryBuy[j - 1];
                                        j--;
                                    }
                                    temporaryBuy[j] = temporary;
                                }
                            }
                            for (int j = 0; j < temporarySell.Count; j++)
                            {
                                if (temporarySell[j].StopLoss == 0)
                                {
                                    if (j < temporarySell.Count - 1)
                                    {
                                        if (temporarySell[j + 1].StopLoss < temporaryBuy[0].StopLoss)
                                        {
                                            temporarySell[j].StopLoss = temporaryBuy[0].StopLoss;
                                        }
                                        else
                                        {
                                            temporarySell[j].StopLoss = temporaryBuy[0].StopLoss;
                                        }
                                    }
                                    else
                                    {
                                        temporarySell[j].StopLoss = temporaryBuy[0].StopLoss;
                                    }
                                }
                            }
                            if (temporarySell.Count > 1)
                            {
                                for (int k = 0; k < temporarySell.Count - 1; k++)
                                {
                                    int j = k + 1;
                                    Order temporary = temporarySell[j];
                                    while (j > 0 && temporary.StopLoss < temporarySell[j - 1].StopLoss)
                                    {
                                        temporarySell[j] = temporarySell[j - 1];
                                        j--;
                                    }
                                    temporarySell[j] = temporary;
                                }
                            }
                        }
                        for (int j = 0; j < temporaryBuy.Count; j++)
                        {
                            for (int l = 0; l < temporarySell.Count; l++)
                            {
                                if (temporaryBuy.Count > 0 && temporarySell.Count > 0)
                                {
                                    if (((temporarySell[l].StopLoss <= temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss) || temporarySell[l].StopLoss == 0 || temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss == 0))
                                    {
                                        if (temporarySell[l].Amount > temporaryBuy[temporaryBuy.Count - 1 - j].Amount)
                                        {
                                            listOfOrders.Add(new Order((temporaryBuy[temporaryBuy.Count - 1 - j].Amount), i, "JustDoIt", -1, "Buy", temporaryBuy[temporaryBuy.Count - 1 - j].UserId));
                                            listOfOrders.Add(new Order((temporaryBuy[temporaryBuy.Count - 1 - j].Amount), i, "JustDoIt", -1, "Sell", temporarySell[l].UserId));
                                            if (temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss != 0)
                                            {
                                                dividedNumber += (temporaryBuy[temporaryBuy.Count - 1 - j].Amount * temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss);
                                                dividedBy += temporaryBuy[temporaryBuy.Count - 1 - j].Amount;
                                                if (temporarySell[l].StopLoss != 0)
                                                {
                                                    dividedNumber += (temporaryBuy[temporaryBuy.Count - 1 - j].Amount * temporarySell[l].StopLoss);
                                                    dividedBy += temporaryBuy[temporaryBuy.Count - 1 - j].Amount;
                                                }
                                            }
                                            temporarySell[l].Amount -= temporaryBuy[temporaryBuy.Count - 1 - j].Amount;
                                            temporaryBuy.RemoveAt(temporaryBuy.Count - 1 - j);
                                            l--;
                                        }
                                        else if (temporarySell[l].Amount == temporaryBuy[temporaryBuy.Count - 1 - j].Amount)
                                        {
                                            listOfOrders.Add(new Order((temporarySell[l].Amount), i, "JustDoIt", -1, "Buy", temporaryBuy[temporaryBuy.Count - 1 - j].UserId));
                                            listOfOrders.Add(new Order((temporarySell[l].Amount), i, "JustDoIt", -1, "Sell", temporarySell[l].UserId));
                                            if (temporarySell[l].StopLoss != 0)
                                            {
                                                dividedNumber += (temporarySell[l].Amount * temporarySell[l].StopLoss);
                                                dividedBy += temporarySell[l].Amount;
                                                if (temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss != 0)
                                                {
                                                    dividedNumber += (temporarySell[l].Amount * temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss);
                                                    dividedBy += temporarySell[l].Amount;
                                                }
                                            }
                                            temporarySell.RemoveAt(l);
                                            temporaryBuy.RemoveAt(temporaryBuy.Count - 1 - j);
                                            l--;
                                        }
                                        else
                                        {
                                            listOfOrders.Add(new Order((temporarySell[l].Amount), i, "JustDoIt", -1, "Buy", temporaryBuy[temporaryBuy.Count - 1 - j].UserId));
                                            listOfOrders.Add(new Order((temporarySell[l].Amount), i, "JustDoIt", -1, "Sell", temporarySell[l].UserId));
                                            if (temporarySell[l].StopLoss != 0)
                                            {
                                                dividedNumber += (temporarySell[l].Amount * temporarySell[l].StopLoss);
                                                dividedBy += temporarySell[l].Amount;
                                                if (temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss != 0)
                                                {
                                                    dividedNumber += (temporarySell[l].Amount * temporaryBuy[temporaryBuy.Count - 1 - j].StopLoss);
                                                    dividedBy += temporarySell[l].Amount;
                                                }
                                            }
                                            temporaryBuy[temporaryBuy.Count - 1 - j].Amount -= temporarySell[l].Amount;
                                            temporarySell.RemoveAt(l);
                                            l--;
                                        }
                                    }
                                }
                            }
                        }
                        while (counter2 < listOfOrders.Count && listOfOrders[counter2].Index == i && listOfOrders[counter2].StopLoss != -1)
                        {
                            if (!myContains(listOfOrders[counter2], temporaryBuy) && !myContains(listOfOrders[counter2], temporarySell))
                            {
                                listOfOrders.RemoveAt(counter2);
                                counter2--;
                                counter--;
                            }
                            counter2++;
                        }
                        double price = 0;
                        if (dividedBy != 0)
                        {
                            price = dividedNumber / dividedBy;
                            listView.items[i].Price = price;                     
                        }

                        for (int l = 0; l < listOfOrders.Count; l++)
                        {
                            if (listOfOrders[l].StopLoss == -1)
                            {
                                if (listOfOrders[l].BuyOrSell == "Buy")
                                {
                                    //BUY
                                    listView.BuyStock(listOfOrders[l].Index, listOfOrders[l].Amount,listOfOrders[l].UserId);
                                }
                                else if (listOfOrders[l].BuyOrSell == "Sell")
                                {
                                    //SELL
                                    listView.SellStock(listOfOrders[l].Index, listOfOrders[l].Amount,listOfOrders[l].UserId);
                                }
                                listOfOrders.RemoveAt(l);
                                l--;
                            }
                        }
                    }
                    dividedBy = 0;
                    dividedNumber = 0;
                    temporaryBuy.Clear();
                    temporarySell.Clear();
                }
                for (int i = 0; i < listView.items.Count; i++)
                {
                    long ticks = new DateTime(2015, 12, 1).Ticks;
                    ticks += (TimeSpan.TicksPerDay * listView.items[i].priceHistory.Count);
                    listView.items[i].dateHistory.Add(new DateTime(ticks));
                    listView.items[i].priceHistory.Add(listView.items[i].Price);
                }
                if (obj != null && selectedIndex >= 0)
                {
                    dateList = obj.dateHistory;
                    priceList = obj.priceHistory;
                    MyUpdate = 1;//pusti se hlavni thread, kde se vykresli zmeny v cenach do grafu a obchodnici odeslou svoje nabidky a poptavky
                }
                DateTime after = DateTime.Now;
                TimeSpan timespan = after - before;
                if (timespan.TotalMilliseconds <= 29000)
                {
                    Thread.Sleep(30000 - (int)timespan.TotalMilliseconds);
                }
                else
                {
                    Thread.Sleep(1000);
                }
                //Thread.Sleep(30000);
            }
        }

        private bool myContains(Order order, List<Order> orders)
        {
            foreach (var item in orders)
            {
                if (item.BuyOrSell == order.BuyOrSell && item.Index == order.Index && item.StopLoss == order.StopLoss && item.Type == order.Type && item.UserId == order.UserId)
                {
                    if (item.Amount != order.Amount)
                    {
                        order.Amount = item.Amount;
                    }
                    return true;
                }
                else if (item.BuyOrSell == order.BuyOrSell && item.Index == order.Index && item.Type == "MarketOrder" && item.Type == "MarketOrder" && item.UserId == order.UserId)
                {
                    if (item.Amount != order.Amount)
                    {
                        order.Amount = item.Amount;
                    }
                    if (item.StopLoss != 0)
                    {
                        item.StopLoss = 0;
                    }
                    return true;
                }
            }
            return false;
        }

        private void GetEvent()
        {
                Random rnd = new Random();
                int selected;
                if (eventsIDArchive.Count != 0)
                {
                    do
                    {
                        selected = rnd.Next(listView.events.Count);
                    } while (eventsIDArchive.Contains(selected));
                }
                else
                {
                    selected = rnd.Next(listView.events.Count);
                }
                eventsIDArchive.Add(selected);
                eventsArchive.Add(listView.events[selected]);

                textBlockNews.Text = eventsArchive[eventsArchive.Count - 1].Description;
                newsImage.Source = newsImages[eventsArchive[eventsArchive.Count - 1].ID - 1];
        }
        private void SetPopularity()
        {
            int stock = eventsArchive[eventsArchive.Count - 1].Stock;
            int influence = (int)eventsArchive[eventsArchive.Count - 1].Influence;
            string country = eventsArchive[eventsArchive.Count - 1].Country;
            string industry = eventsArchive[eventsArchive.Count - 1].Industry;

            if (stock != 0)
            {
                listView.items[stock - 1].Popularity += influence;
            }
            else if (industry != "All")
            {
                foreach (Stocks item in listView.items)
                {
                    if (item.Industry == industry)
                    {
                        item.Popularity += influence;
                    }
                }
            }
            else if (country != "All")
            {
                foreach (Stocks item in listView.items)
                {
                    if (item.Country == country)
                    {
                        item.Popularity += influence;
                    }
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + MainWindow.filenameS + ";Persist Security Info=False;");
                string queryString;
                OleDbCommand cmd;

                connection.Open();
                for (int i = 0; i < listView.users.Count; i++)
                {
                    if (listView.users[i].ID < 200)
                    {
                        queryString = "UPDATE Users SET Users.[Money] = " + Convert.ToInt32(listView.users[0].Money) + " WHERE(((Users.ID) = " + listView.users[0].ID.ToString() + "))";
                    }
                    else
                    {
                        queryString = "UPDATE Users SET Users.[Money] = " + Convert.ToInt32(listView.users[(listView.users[i].ID % 200) + 1].Money) + " WHERE(((Users.ID) = " + listView.users[i].ID.ToString() + "))";
                    }

                    cmd = new OleDbCommand(queryString, connection);
                    cmd.ExecuteNonQuery();
                }

                connection.Close();


                queryString = "UPDATE Users SET Users.[CompletedQuests] = " + Convert.ToInt32(listView.users[0].CompletedQuests) + " WHERE(((Users.ID) = " + listView.users[0].ID.ToString() + "))";
                cmd = null;
                cmd = new OleDbCommand(queryString, connection);
                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();


                connection.Open();
                for (int i = 0; i < listView.items.Count; i++)
                {
                    for (int j = 0; j < listView.items[i].priceHistory.Count; j++)
                    {
                        queryString = "INSERT INTO PriceHistory ([Stock], [Date], [Price], [User]) VALUES ('" + (listView.items[i].ID + 1).ToString() + "','" + listView.items[i].dateHistory[j].ToString() + "', '" + listView.items[i].priceHistory[j].ToString() + "', '" + listView.users[0].ID.ToString() + "')";
                        cmd = null;
                        cmd = new OleDbCommand(queryString, connection);
                        cmd.ExecuteNonQuery();
                    }
                }
                connection.Close();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
