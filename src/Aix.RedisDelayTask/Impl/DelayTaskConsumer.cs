using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Aix.RedisDelayTask.Foundation;
using System.Threading;
using StackExchange.Redis;
using Aix.RedisDelayTask.Utils;
using Aix.RedisDelayTask.Model;

namespace Aix.RedisDelayTask.Impl
{
    public class DelayTaskConsumer
    {
        private IServiceProvider _serviceProvider;
        private string _delayTopicName;
        private ILogger<DelayTaskConsumer> _logger;
        private RedisDelayTaskOptions _options;
        private RedisStorage _redisStorage;
        private int BatchCount = 100; //一次拉取多少条
        private int PreReadSecond = 10; //提前读取多长数据
        private readonly IDelayTaskLifetime _delayTaskLifetime;

        private RepeatChecker _repeatStartChecker = new RepeatChecker();

        public event Func<ConsumeDelayTaskResult, Task<bool>> OnMessage;

        CancellationToken StoppingToken => _delayTaskLifetime.Stopping;
        public DelayTaskConsumer(IServiceProvider serviceProvider, string delayTopicName)
        {
            _serviceProvider = serviceProvider;
            _delayTopicName = delayTopicName;
            _logger = _serviceProvider.GetService<ILogger<DelayTaskConsumer>>();
            _options = _serviceProvider.GetService<RedisDelayTaskOptions>();
            _delayTaskLifetime = _serviceProvider.GetService<IDelayTaskLifetime>();
            _redisStorage = _serviceProvider.GetService<RedisStorage>();

        }

        public Task Start(CancellationToken cancellationToken = default)
        {
            if (!_repeatStartChecker.Check()) return Task.CompletedTask;

            Task.Run(async () =>
            {
                while (!StoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Execute();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (RedisException ex)
                    {
                        _logger.LogError(ex, "Aix.RedisDelayTask错误");
                        await TaskEx.DelayNoException(TimeSpan.FromSeconds(10), StoppingToken);
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Aix.RedisDelayTask执行任务异常，暂停1秒";
                        _logger.LogError(ex, errorMsg);
                        await TaskEx.DelayNoException(TimeSpan.FromSeconds(1), StoppingToken);
                    }

                }
            });

            return Task.CompletedTask;
        }

        public async Task Execute()
        {
            var lockKey = Helper.GetDelayLock(_options, _delayTopicName);
            long delay = 0; //毫秒
            await _redisStorage.Lock(lockKey, TimeSpan.FromSeconds(_options.DelayTaskPullLockSecond), async () =>
           {
               var now = DateTime.Now;
               var maxScore = DateUtils.GetTimeStamp(now);
               var list = await _redisStorage.GetTopDueDealyJobId(_delayTopicName, maxScore + PreReadSecond * 1000, BatchCount); //多查询1秒的数据，便于精确控制延迟

                foreach (var item in list)
               {
                   StoppingToken.ThrowIfCancellationRequested();
                   if (item.Value > maxScore) //预拉去了PreReadSecond秒的数据，可能有还没到时间的
                    {
                       delay = item.Value - maxScore;
                       break;
                   }

                   var jobId = item.Key;
                    // 延时任务到期加入即时任务队列
                    var hashEntities = await _redisStorage.HashGetAll(Helper.GetJobHashId(_options, jobId));//这里要出错了呢
                    TaskData taskData = ToTaskData(hashEntities);
                    //执行任务
                    await ExecuteDelayTask(taskData, jobId);
               }

               if (list.Count == 0)//没有数据时
                {
                   delay = PreReadSecond * 1000;
               }

           }, async () => await TaskEx.DelayNoException(PreReadSecond * 1000, StoppingToken)); //出现并发也休息一会

            if (delay > 0)
            {
                var minDelay = Math.Min((int)delay, PreReadSecond * 1000);
                _redisStorage.WaitForDelayJob(TimeSpan.FromMilliseconds(minDelay), StoppingToken);
            }
        }

        private async Task ExecuteDelayTask(TaskData taskData, string id)
        {
            if (taskData == null)
            {
                _logger.LogError("Aix.RedisDelayTask延迟任务解析出错为空，这里就从hash中删除了");
                await _redisStorage.RemoveNullDealyTask(_delayTopicName, id);
                return;
            }

            //执行任务
            var isSuccess = await HandleMessage(taskData);
            if (isSuccess)
            {
                await _redisStorage.SetDealyTaskSuccess(_delayTopicName, taskData);
            }
            else //重试
            {
                await HandleRetry(taskData);
            }
        }

        /// <summary>
        /// 触发订阅事件
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns>false需要进行重试</returns>
        private async Task<bool> HandleMessage(TaskData taskData)
        {
            var isSuccess = true;
            try
            {
                var result = new ConsumeDelayTaskResult
                {
                    Id = taskData.Id,
                    TaskGroup = taskData.TaskGroup,
                    TaskContent = taskData.TaskContent,
                    ErrorRetryCount = taskData.ErrorRetryCount
                };
                isSuccess = await OnMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aix.RedisDelayTask消费失败,topic:{taskData.TaskContent}");
                isSuccess = false;
            }
            return isSuccess;

        }

        /// <summary>
        /// 处理重试
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns></returns>
        private async Task HandleRetry(TaskData taskData)
        {
            if (taskData.ErrorRetryCount < _options.MaxErrorReTryCount) //需要重试
            {
                var delaySecond = Helper.GetDelaySecond(_options, taskData.ErrorRetryCount);
                taskData.ResetExecuteTime(TimeSpan.FromSeconds(delaySecond));
                taskData.ErrorRetryCount++;

                await _redisStorage.EnqueueDealy(taskData, TimeSpan.FromSeconds(delaySecond));
                _logger.LogInformation($"Aix.RedisDelayTask消费失败,{taskData.TaskContent},{delaySecond}秒后将进行{taskData.ErrorRetryCount }次重试");
            }
        }

        private TaskData ToTaskData(HashEntry[] hashEntities)
        {
            TaskData taskData = null;
            try
            {
                taskData = TaskData.ToTaskData(hashEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Aix.RedisDelayTask解析延迟任务数据报错");
            }
            return taskData;
        }

    }
}
