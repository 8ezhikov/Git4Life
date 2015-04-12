using System;
using System.ServiceModel;
using System.Windows;
using System.Configuration;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri( ConfigurationManager.AppSettings["addr"]);
            host = new ServiceHost(typeof(ChatService), uri);
            host.Open();
            //Console.WriteLine("Chat service listen on endpoint {0}", uri.ToString());
            //Console.WriteLine("Press ENTER to stop chat service...");
            //Console.ReadLine();
     
        }
    }
}
