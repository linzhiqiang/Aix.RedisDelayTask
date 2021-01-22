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
    public class DelayTaskProduerService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DelayTaskProduerService> _logger;

        private readonly IHostEnvironment _hostEnvironment;

        private readonly IDelayTaskService _delayTaskService;
        private CmdOptions _cmdOptions;
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();

        public DelayTaskProduerService(IServiceProvider serviceProvider, ILogger<DelayTaskProduerService> logger, IHostEnvironment hostEnvironment,
            IDelayTaskService delayTaskService,
            CmdOptions cmdOptions)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _delayTaskService = delayTaskService;
            _cmdOptions = cmdOptions;

        }
        public  Task StartAsync(CancellationToken cancellationToken)
        {
            

            Task.Run(async () =>
            {
                await Producer();


            });

            return Task.CompletedTask;

        }

        private async Task Producer()
        {
            int producerCount = _cmdOptions.Count > 0 ? _cmdOptions.Count : 1;
            try
            {
                for (int i = 0; i < producerCount; i++)
                {
                    if (_stoppingSource.Token.IsCancellationRequested) break;
                    try
                    {
                        var messageData = new BusinessMessage
                        {
                            MessageId = i.ToString(),
                            Content = $"我是内容_{i}",
                            CreateTime = DateTime.Now
                        };

                        //json
                        //  await _delayTaskService.PublishAsync(JsonUtils.ToJson( messageData), TimeSpan.FromSeconds(4));

                        //二进制
                        var bytesData = Encoding.UTF8.GetBytes(JsonUtils.ToJson(messageData));
                        await _delayTaskService.PublishAsync(bytesData, TimeSpan.FromSeconds(4));
                        _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}生产数据：MessageId={messageData.MessageId}");
                        //await Task.Delay(5);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "生产消息出错");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
