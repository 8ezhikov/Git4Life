using System.Windows;
using Honeycomb;

namespace CrawlerClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string LocalIP;
        private string GlobalIP;
        public MainWindow()
        {
            InitializeComponent();
            LocalIP = ClientHelper.GetLocalIP();
            GlobalIP = ClientHelper.GetPublicIP();

            LocalIpLabel.Content = "Your local IP is " + LocalIP; 
            GlobalIpLabel.Content = "Your Global IP is " + GlobalIP;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var singleTone = ConnectionSingleton.Instance;
            // InstanceContext site = new InstanceContext(singleTone);
            var newPerson = new ClientCrawlerInfo("sdf","sdfds");
            newPerson.ClientName = "Denis Crawler";
            string globalIP = ClientHelper.GetPublicIP();


              newPerson.ServerIP = globalIP;
            //  newPerson.ImageURL = "3434";
            singleTone.Connect(newPerson);

           // singleTone.SayAndClear("HUE", "MOE", false);
            // var proxy = new ChatClient(site);
            //System.Threading.Thread.Sleep(3242);
            // proxy.Join()
            // proxy.Say("HUI PIZDA JIGUR DA");
        }

        private void StartCrawlerButton_Click(object sender, RoutedEventArgs e)
        {
            var crawlerInstance = new CrawlerEngine();
            crawlerInstance.StartCrawlingProcess("http://webometrics.krc.karelia.ru/");
        }
    }
}