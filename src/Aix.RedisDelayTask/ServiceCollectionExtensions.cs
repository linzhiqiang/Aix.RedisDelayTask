using Aix.RedisDelayTask.Impl;
using Aix.RedisDelayTask.Utils;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace Aix.RedisDelayTask
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisDelayTask(this IServiceCollection services, Action<RedisDelayTaskOptions> setupAction)
        {
            //验证
            var options = new RedisDelayTaskOptions();
            if (setupAction != null)
            {
                setupAction(options);
            }
            ValidOptions(options);
            services.AddSingleton(options);
            if (options.ConnectionMultiplexer != null)
            {
                services.AddSingleton(options.ConnectionMultiplexer);
            }
            else if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                var redis = ConnectionMultiplexer.Connect(options.ConnectionString);
                services.AddSingleton(redis);
            }
            else
            {
                throw new Exception("请配置ConnectionMultiplexer或RedisConnectionString参数");
            }

            services.AddSingleton<IDelayTaskLifetime, DelayTaskLifetime>();
            services.AddSingleton<IDelayTaskService, DelayTaskService>();
            services.AddSingleton<RedisStorage>();
            return services;
        }

        private static void ValidOptions(RedisDelayTaskOptions options)
        {
            AssertUtils.IsTrue(options.DelayTopicCount >=1, "请配置DelayTopicCount参数");
            AssertUtils.IsTrue(options.DelayTaskPullLockSecond >=10 && options.DelayTaskPullLockSecond <=300, "DelayTaskPullLockSecond取值范围[10,300]");
            AssertUtils.IsTrue(options.DelayTaskPreReadSecond >= 5 && options.DelayTaskPreReadSecond <= 60, "DelayTaskPreReadSecond取值范围[5,60]");
            AssertUtils.IsTrue(options.DelayTaskExpireHour >=24, "DelayTaskExpireHour最小值是24");
        }
    }
}
