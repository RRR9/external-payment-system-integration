using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace ZudamalZetMobileServices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseWindowsService();
    }

    public class ZetMobileException : Exception
    {
        public ZetMobileException()
        { }

        public ZetMobileException(string message)
            : base(message)
        { }
    }

    public class ZetMobileBadNumberException : Exception
    {
        public ZetMobileBadNumberException()
        { }

        public ZetMobileBadNumberException(string message)
            : base(message)
        { }
    }
}
