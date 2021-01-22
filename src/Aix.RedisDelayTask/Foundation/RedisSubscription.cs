using Aix.RedisDelayTask.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Aix.RedisDelayTask.Foundation
{
    internal class RedisSubscription
    {
        private IServiceProvider _serviceProvider;
        private readonly ManualResetEvent _mre = new ManualResetEvent(false);
        private readonly ISubscriber _subscriber;

        public RedisSubscription(IServiceProvider serviceProvider, ISubscriber subscriber, string subscriberChannel)
        {
            _serviceProvider = serviceProvider;
            Channel = subscriberChannel;

            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
            _subscriber.Subscribe(Channel, (channel, value) =>
            {
                _mre.Set();
            });
        }

        protected RedisSubscription() { }

        public virtual string Channel { get; }

        public virtual void WaitForJob(TimeSpan timeout, CancellationToken cancellationToken)
        {
            _mre.Reset();
            WaitHandle.WaitAny(new[] { _mre, cancellationToken.WaitHandle }, timeout);
        }
    }

    


    //internal class DelayTaskRedisSubscription
    //{
    //    private IServiceProvider _serviceProvider;
    //    private ConnectionMultiplexer _connectionMultiplexer = null;
    //    private RedisDelayTaskOptions _options;

    //    ConcurrentDictionary<string, RedisSubscription> Datas = new ConcurrentDictionary<string, RedisSubscription>();

    //    public DelayTaskRedisSubscription(IServiceProvider serviceProvider, ConnectionMultiplexer connectionMultiplexer,
    //        RedisDelayTaskOptions options)
    //    {
    //        _serviceProvider = serviceProvider;
    //        _connectionMultiplexer = connectionMultiplexer;
    //        _options = options;

    //        foreach (var item in Helper.GetDelayTopicList(_options))
    //        {
    //            Datas.TryAdd(item, new RedisSubscription(_serviceProvider, _connectionMultiplexer.GetSubscriber(), item+"channel"));
    //        }

    //    }

    //    public RedisSubscription GetRedisSubscription(string delayTopicName)
    //    {
    //        Datas.TryGetValue(delayTopicName,out RedisSubscription value );
    //        AssertUtils.IsNotNull(value,"延迟等待订阅为null");
    //        return value;
    //    }


    //}
}
