using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using Serilog;

namespace Honeycomb
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.Seq("http://193.124.113.235:5341")
              .CreateLogger();

            Mapper.Initialize(cfg => cfg.CreateMap<Seed, SeedDTO>());
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Log.Error(e,"");
        }

        //TODO: Fix global app error handling
        public bool DoHandle { get; set; }
        //private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    if (this.DoHandle)
        //    {
        //        //Handling the exception within the UnhandledExcpeiton handler.
        //        MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
        //        e.Handled = true;
        //    }
        //    else
        //    {
        //        //If you do not set e.Handled to true, the application will close due to crash.
        //        MessageBox.Show("Application is going to close! ", "Uncaught Exception");
        //        e.Handled = false;
        //    }
        //}
    }
}
