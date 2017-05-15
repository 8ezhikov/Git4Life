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

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Seed, SeedDTO>();
                cfg.CreateMap<SeedDTO, Seed>();
                cfg.CreateMap<Batch, BatchDTO>();
                cfg.CreateMap<BatchDTO, Batch>();
            });
          
           // Mapper.Initialize(cfg => cfg.CreateMap<Seed, SeedDTO>());

        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception)args.ExceptionObject;
            Log.Error(e,"");
            Log.CloseAndFlush();
        }
    }
}
