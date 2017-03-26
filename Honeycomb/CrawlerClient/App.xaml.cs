using System;
using System.Windows;
using Serilog;

namespace CrawlerClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Seq("http://193.124.113.235:5341")
              .WriteTo.RollingFile(@"D:\1.txt")
              .CreateLogger();

           

        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Log.Error(e, "");
            Log.CloseAndFlush();
        }
    }
}