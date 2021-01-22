using Aix.RedisDelayTask;
using Aix.RedisDelayTaskSample.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTaskSample.HostServices
{
    public class DelayTaskConsumerService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DelayTaskConsumerService> _logger;

        private readonly IHostEnvironment _hostEnvironment;

        private readonly IDelayTaskService _delayTaskService;
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private int Count = 0;
        public DelayTaskConsumerService(IServiceProvider serviceProvider, ILogger<DelayTaskConsumerService> logger, IHostEnvironment hostEnvironment,
            IDelayTaskService delayTaskService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _delayTaskService = delayTaskService;

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await _delayTaskService.SubscribeAsync(Consumer);

            });

            return Task.CompletedTask;
        }

        private async Task<bool> Consumer(ConsumeDelayTaskResult delayTaskResult)
        {
            var current = Interlocked.Increment(ref Count);
            //Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}--{delayTaskResult.TaskContent}");
            if (delayTaskResult.TaskBytesContent != null && delayTaskResult.TaskBytesContent.Length > 0)
            {
                var data = JsonUtils.FromJson<BusinessMessage>(Encoding.UTF8.GetString(delayTaskResult.TaskBytesContent));
                _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}消费数据：{data.Content}----count={current}");
            }

            else
            {
                var data = JsonUtils.FromJson<BusinessMessage>(delayTaskResult.TaskContent);
                _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}消费数据：{data.Content}----count={current}");
            }
            await Task.CompletedTask;
            return true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
