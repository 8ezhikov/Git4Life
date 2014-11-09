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
using Common;

namespace CrawlerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var singleTone = ConnectionSingleton.GetInstance();
            InstanceContext site = new InstanceContext(singleTone);
            var newPerson = new Person();
            newPerson.Name = "TT";
            newPerson.ImageURL = "3434";
            singleTone.Connect(newPerson);
            singleTone.SayAndClear("HUE","MOE", false);
           // var proxy = new ChatClient(site);
           //System.Threading.Thread.Sleep(3242);
           // proxy.Join()
           // proxy.Say("HUI PIZDA JIGUR DA");
        }
    }
}
