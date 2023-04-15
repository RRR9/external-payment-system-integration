using log4net;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZudamalZetMobileServices
{
    public class Worker : BackgroundService
    {
        static private readonly ILog _log = LogManager.GetLogger(typeof(Worker));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.Info("Start:");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Service.Start();
                }
                catch(Exception ex)
                {
                    _log.Error(ex);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
