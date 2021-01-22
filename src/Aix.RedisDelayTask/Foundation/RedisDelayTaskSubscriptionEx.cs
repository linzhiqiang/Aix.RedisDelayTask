using Aix.RedisDelayTask.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Aix.RedisDelayTask.Foundation
{
    internal class RedisDelayTaskSubscriptionEx
    {
        private IServiceProvider _serviceProvider;
        private ConnectionMultiplexer _connectionMultiplexer = null;
        private readonly ISubscriber _subscriber;
        private RedisDelayTaskOptions _options;
        ConcurrentDictionary<string, ManualResetEvent> Datas = new ConcurrentDictionary<string, ManualResetEvent>();
        private RepeatChecker _repeatSubscribeChecker = new RepeatChecker();

        public RedisDelayTaskSubscriptionEx(IServiceProvider serviceProvider, ConnectionMultiplexer connectionMultiplexer, RedisDelayTaskOptions options)
        {
            _serviceProvider = serviceProvider;
            _connectionMultiplexer = connectionMultiplexer;
            _subscriber = _connectionMultiplexer.GetSubscriber();
            _options = options;// _serviceProvider.GetService<RedisDelayTaskOptions>();
            Channel = Helper.GetDelayChannel(_options);

            foreach (var item in Helper.GetDelayTopicList(_options))
            {
                Datas.TryAdd(item, new ManualResetEvent(false));
            }
        }

        public Task StartSubscribe()
        {
            if (_options.PreReadMillisecond <= 1000) return Task.CompletedTask;//小于1秒就不用通过订阅通知了
            if (!_repeatSubscribeChecker.Check()) return Task.CompletedTask;

            _subscriber.Subscribe(Channel, (channel, value) => //这里value是延迟任务的delayTopicName
            {
                //Console.WriteLine("收到订阅"+ value);
                GetManualResetEvent(value).Set();
            });

            return Task.CompletedTask;
        }


        public virtual string Channel { get; }

        public virtual void WaitForJob(string delayTopicName, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var manualResetEvent = GetManualResetEvent(delayTopicName);
            manualResetEvent.Reset();
            WaitHandle.WaitAny(new[] { manualResetEvent, cancellationToken.WaitHandle }, timeout);
        }

        private ManualResetEvent GetManualResetEvent(string delayTopicName)
        {
            Datas.TryGetValue(delayTopicName, out ManualResetEvent manualResetEvent);
            AssertUtils.IsNotNull(manualResetEvent, "延迟等待锁为null");
            return manualResetEvent;
        }
    }
}
