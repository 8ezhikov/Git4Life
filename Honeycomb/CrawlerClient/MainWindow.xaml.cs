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
        }

        private void StartCrawlerButton_Click(object sender, RoutedEventArgs e)
        {
            var crawlerInstance = new CrawlerEngine();
            crawlerInstance.StartCrawlingProcess("http://webometrics.krc.karelia.ru/");
        }
    }
}