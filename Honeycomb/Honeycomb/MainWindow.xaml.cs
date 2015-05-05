using System;
using System.ServiceModel;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;

namespace Honeycomb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        ServiceHost host;

        public MainWindow()
        {
            InitializeComponent();
            OutputTextBox.AppendText("Starting Server...\n");
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri( ConfigurationManager.AppSettings["addr"]);
            host = new ServiceHost(typeof(RemoteCrawlerService), uri);
            host.Open();
            OutputTextBox.AppendText("Chat service listen on endpoint " + uri + "\n");
            OutputTextBox.AppendText("Press ENTER to stop chat service... \n");
            //Console.ReadLine();
     
        }
    }
}
