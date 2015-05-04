using System.Windows;
using Honeycomb;

namespace CrawlerClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSingleton singleTone = ConnectionSingleton.Instance;
            // InstanceContext site = new InstanceContext(singleTone);
            var newPerson = new ClientCrawlerInfo();
            newPerson.ClientName = "Denis Crawler";
            string localIP = ClientHelper.GetLocalIP();
            string globalIP = ClientHelper.GetPublicIP();


            //  newPerson.ServerIP = DataFormat;
            //  newPerson.ImageURL = "3434";
            singleTone.Connect(newPerson);
            singleTone.SayAndClear("HUE", "MOE", false);
            // var proxy = new ChatClient(site);
            //System.Threading.Thread.Sleep(3242);
            // proxy.Join()
            // proxy.Say("HUI PIZDA JIGUR DA");
        }

        private void StartCrawlerButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}