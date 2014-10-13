using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Honeycomb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceHost host;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri("http://localhost:22222/chatservice");
            host = new ServiceHost(typeof(Chatters.ChatService), uri);
            host.Open();
            //Console.WriteLine("Chat service listen on endpoint {0}", uri.ToString());
            //Console.WriteLine("Press ENTER to stop chat service...");
            //Console.ReadLine();
     
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            host.Abort();
            host.Close(); 

        }
    }
}
