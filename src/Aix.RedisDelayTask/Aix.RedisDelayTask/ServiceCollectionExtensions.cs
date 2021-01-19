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
                throw new Exception("ConnectionMultiplexer或RedisConnectionString为空");
            }

            services.AddSingleton<IDelayTaskLifetime, DelayTaskLifetime>();
            services.AddSingleton<IDelayTaskService, DelayTaskService>();
            services.AddSingleton<RedisStorage>();
            return services;
        }

        private static void ValidOptions(RedisDelayTaskOptions options)
        {
            
        }
    }
}
