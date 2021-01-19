using Aix.RedisDelayTask.Foundation;
using Aix.RedisDelayTask.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTask.Impl
{
    public class DelayTaskService : IDelayTaskService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DelayTaskService> _logger;
        private readonly IDelayTaskLifetime _delayTaskLifetime;
        private RedisDelayTaskOptions _options;
        private RedisStorage _redisStorage;

        private RepeatChecker _repeatStartChecker = new RepeatChecker();
        private RepeatChecker _repeatStopChecker = new RepeatChecker();

        List<DelayTaskConsumer> _delayTaskConsumers = new List<DelayTaskConsumer>();

        public DelayTaskService(IServiceProvider serviceProvider,
            ILogger<DelayTaskService> logger,
            IDelayTaskLifetime delayTaskLifetime,
            RedisDelayTaskOptions options,
            RedisStorage redisStorage)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _delayTaskLifetime = delayTaskLifetime;
            _options = options;
            _redisStorage = redisStorage;
        }



        public async Task<string> PublishDelayAsync(string taskGroup, string taskContent, TimeSpan delay)
        {
            var taskData = TaskData.Create(taskGroup, taskContent, delay);
            await _redisStorage.EnqueueDealy(taskData, delay);
            return taskData.Id;
        }

        public async Task SubscribeAsync(Func<ConsumeDelayTaskResult, Task<bool>> handler, CancellationToken cancellationToken = default)
        {
            if (!_repeatStartChecker.Check()) return;
            foreach (var item in Helper.GetDelayTopicList(this._options))
            {
                DelayTaskConsumer delayTaskConsumer = new DelayTaskConsumer(this._serviceProvider, item);
                delayTaskConsumer.OnMessage += handler;
                _delayTaskConsumers.Add(delayTaskConsumer);
            }

            foreach (var item in _delayTaskConsumers)
            {
                await item.Start();
            }

            _delayTaskLifetime.NotifyStarted();



            await Task.CompletedTask;
        }



        public void Dispose()
        {
            if (!_repeatStopChecker.Check()) return;

            //开始stop
            _delayTaskLifetime.Stop();

            //开始stop 具体需要stop的


            //stop结束通知
            _delayTaskLifetime.NotifyStopped();
        }

        #region  private 


        #endregion
    }
}
