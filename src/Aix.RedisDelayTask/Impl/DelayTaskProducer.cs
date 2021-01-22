using Aix.RedisDelayTask.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Aix.RedisDelayTask.Impl
{
    internal class DelayTaskProducer
    {
        private IServiceProvider _serviceProvider;
        private ILogger<DelayTaskProducer> _logger;
        private RedisDelayTaskOptions _options;
        private RedisStorage _redisStorage;

        public DelayTaskProducer(IServiceProvider serviceProvider, ILogger<DelayTaskProducer> logger, 
            RedisDelayTaskOptions options,
            RedisStorage redisStorage)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
            _redisStorage = redisStorage;


        }
        public async Task<bool> EnqueueDealy(TaskData taskData, TimeSpan delay)
        {
            return await _redisStorage.EnqueueDealy(taskData, delay);
        }
    }
}
