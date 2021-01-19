using Aix.RedisDelayTask.Foundation;
using Aix.RedisDelayTask.Model;
using Aix.RedisDelayTask.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTask.Impl
{
    public class RedisStorage
    {
        private IServiceProvider _serviceProvider;
        private ConnectionMultiplexer _redis = null;
        private IDatabase _database;
        private RedisDelayTaskOptions _options;
        private readonly RedisSubscription _delayJobChannelSubscription;

        public RedisStorage(IServiceProvider serviceProvider, ConnectionMultiplexer redis, RedisDelayTaskOptions options)
        {
            _serviceProvider = serviceProvider;
            this._redis = redis;
            this._options = options;
            _database = redis.GetDatabase();

            _delayJobChannelSubscription = new RedisSubscription(_serviceProvider, _redis.GetSubscriber(), Helper.GetDelayChannel(_options));
        }

        public async Task<bool> EnqueueDealy(TaskData taskData,TimeSpan delay)
        {
          var executeTime =   DateUtils.GetTimeStamp(DateTime.Now.Add(delay));

            var values = taskData.ToHashEntries();
            var hashJobId = Helper.GetJobHashId(_options, taskData.Id);
            var delayTopic = Helper.GetDelayTopic(_options, taskData.Id);
            var trans = _database.CreateTransaction();
#pragma warning disable CS4014
            trans.HashSetAsync(hashJobId, values.ToArray());
            trans.KeyExpireAsync(hashJobId, delay.Add(TimeSpan.FromDays(7)));
            trans.SortedSetAddAsync(delayTopic, taskData.Id, executeTime); //当前时间戳，
#pragma warning restore CS4014
            var result = await trans.ExecuteAsync();

            //时间很短的 通过发布订阅及时通知
            if (delay < TimeSpan.FromSeconds(_options.DelayTaskPreReadSecond))
            {
                await _database.PublishAsync(_delayJobChannelSubscription.Channel, delayTopic);
            }

            return result;
        }

        /// <summary>
        /// 查询到期的延迟任务
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Task<IDictionary<string, long>> GetTopDueDealyJobId(string delayTopicName, long timeStamp, int count)
        {
            var nowTimeStamp = timeStamp;
            // var result = _database.SortedSetRangeByScoreWithScores(Helper.GetDelaySortedSetName(_options), double.NegativeInfinity, nowTimeStamp, Exclude.None, Order.Ascending, 0, count);
            var result = _database.SortedSetRangeByScoreWithScores(delayTopicName, double.NegativeInfinity, nowTimeStamp, Exclude.None, Order.Ascending, 0, count);
            IDictionary<string, long> dict = new Dictionary<string, long>();
            foreach (SortedSetEntry item in result)
            {
                dict.Add(item.Element, (long)item.Score);
            }
            return Task.FromResult(dict);
        }

        public async Task<bool> SetDealyTaskSuccess(string delayTopicName, TaskData taskData)
        {
            var id = taskData.Id;

            var trans = _database.CreateTransaction();
#pragma warning disable CS4014
            trans.SortedSetRemoveAsync(delayTopicName, id);
            trans.KeyDeleteAsync(Helper.GetJobHashId(_options, id));
#pragma warning restore CS4014
            var result = await trans.ExecuteAsync();
            return result;
        }

        /// <summary>
        /// 删除数据为空的 延迟任务数据  一般不会有这种情况的
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveNullDealyTask(string delayTopicName, string id)
        {
            var trans = _database.CreateTransaction();
#pragma warning disable CS4014
            //trans.SortedSetRemoveAsync(Helper.GetDelaySortedSetName(_options), jobId);
            trans.SortedSetRemoveAsync(delayTopicName, id);
            trans.KeyDeleteAsync(Helper.GetJobHashId(_options, id));
#pragma warning restore CS4014
            var result = await trans.ExecuteAsync();

            return result;
        }

        public void WaitForDelayJob(TimeSpan timeSpan, CancellationToken cancellationToken = default(CancellationToken))
        {
            _delayJobChannelSubscription.WaitForJob(timeSpan, cancellationToken);
        }

        public async Task<HashEntry[]> HashGetAll(string key)
        {
            var hashEntities = await _database.HashGetAllAsync(key);
            return hashEntities;
        }

        public async Task Lock(string key, TimeSpan span, Func<Task> action, Func<Task> concurrentCallback = null)
        {
            string token = Guid.NewGuid().ToString();
            if (LockTake(key, token, span))
            {
                try
                {
                    await action();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    LockRelease(key, token);
                }
            }
            else
            {
                if (concurrentCallback != null) await concurrentCallback();
                else throw new Exception($"出现并发key:{key}");
            }
        }

        #region private

        /// <summary>
        /// 获取一个锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        private bool LockTake(string key, string token, TimeSpan expiry)
        {
            return _database.LockTake(key, token, expiry);
        }

        /// <summary>
        /// 释放一个锁(需要token匹配)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool LockRelease(string key, string token)
        {
            return _database.LockRelease(key, token);
        }

        #endregion
    }
}
