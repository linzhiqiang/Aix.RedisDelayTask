using Aix.RedisDelayTask;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTaskSample.HostServices
{
    public class StartHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StartHostService> _logger;

        private readonly IHostEnvironment _hostEnvironment;

        private readonly IDelayTaskService _delayTaskService;

        public StartHostService(IServiceProvider serviceProvider, ILogger<StartHostService> logger, IHostEnvironment hostEnvironment,
            IDelayTaskService delayTaskService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _delayTaskService = delayTaskService;

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            

            Task.Run(async () =>
            {
                await _delayTaskService.SubscribeAsync(Consumer);
                await _delayTaskService.PublishDelayAsync("taskGroup", "123", TimeSpan.FromSeconds(5));
            });

        }

        private async Task<bool> Consumer(ConsumeDelayTaskResult delayTaskResult)
        {
            Console.WriteLine(delayTaskResult.TaskContent);
            return true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
