using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTask
{
  public  class RedisDelayTaskOptions
    {
        private int[] DefaultRetryStrategy = new int[] { 1, 5, 10, 30, 60, 2 * 60, 5 * 60, 10 * 60 };

        public RedisDelayTaskOptions()
        {
            this.TopicPrefix = "aix:delaytask";
        }

        /// <summary>
        /// RedisConnectionString和ConnectionMultiplexer传一个即可
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        ///  RedisConnectionString和ConnectionMultiplexer传一个即可
        /// </summary>
        public ConnectionMultiplexer ConnectionMultiplexer { get; set; }

        /// <summary>
        /// 前缀，为了防止重复，建议用项目名称
        /// </summary>
        public string TopicPrefix { get; set; }

        /// <summary>
        /// 延迟队列数量 进行分区
        /// </summary>
        public int DelayTopicCount { get; set; } = 1;

        /// <summary>
        /// 延迟任务预处数据时间 内部有订阅延迟时间小于该值的任务，所以也支持小于该值的任务  默认10秒（不建议更小，可以更大）
        /// </summary>
        public int DelayTaskPreReadSecond { get; set; } = 10;

        /// <summary>
        /// 最大错误重试次数 默认10次
        /// </summary>
        public int MaxErrorReTryCount { get; set; } = 10;

        /// <summary>
        /// 失败重试延迟策略 单位：秒 ,不要直接调用请调用GetRetryStrategy()  默认失败次数对应值延迟时间[ 1, 10, 30, 60, 2 * 60, 2 * 60, 2 * 60, 5 * 60, 5 * 60,10*60   ];
        /// </summary>
        public int[] RetryStrategy { get; set; }

        public int[] GetRetryStrategy()
        {
            if (RetryStrategy == null || RetryStrategy.Length == 0) return DefaultRetryStrategy;
            return RetryStrategy;
        }
    }
}
