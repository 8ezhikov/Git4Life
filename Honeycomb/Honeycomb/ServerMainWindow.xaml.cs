using System;
using System.ServiceModel;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;

namespace Honeycomb
{
    /// <summary>
    /// Interaction logic for ServerMainWindow.xaml
    /// </summary>
    public partial class ServerMainWindow
    {
        ServiceHost host;
        private RemoteCrawlerService instance;
        public ServerMainWindow()
        {
            InitializeComponent();
            OutputTextBox.AppendText("Starting Server...\n");
            var uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            instance = new RemoteCrawlerService();
            host = new ServiceHost(instance, uri);
            host.Open();
            OutputTextBox.AppendText("Chat service listen on endpoint " + uri + "\n");
            OutputTextBox.AppendText("Press ENTER to stop chat service... \n");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var hoster = ((RemoteCrawlerService) host.SingletonInstance);
            hoster.StartCrawling();
        }
    }
}
