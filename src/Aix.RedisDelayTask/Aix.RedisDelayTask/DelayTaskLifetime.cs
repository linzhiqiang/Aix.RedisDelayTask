using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.RedisDelayTask
{
    /// <summary>
    /// 开始时调用 NotifyStarted, 停止时调用Stop,停止的最后调用NotifyStopped
    /// </summary>
    public interface IDelayTaskLifetime
    {
        /// <summary>
        /// 开始时 token
        /// </summary>
        CancellationToken ScheduleTaskStarted { get; }

        /// <summary>
        /// 开始停止token 业务判断请使用这个属性
        /// </summary>
        CancellationToken ScheduleTaskStopping { get; }

        /// <summary>
        /// 停止结束后token
        /// </summary>
        CancellationToken ScheduleTaskStopped { get; }

        /// <summary>
        /// 主动停止
        /// </summary>
        void Stop();

        /// <summary>
        /// 开始完成通知
        /// </summary>
        void NotifyStarted();

        /// <summary>
        /// 停止结束通知
        /// </summary>
        void NotifyStopped();
    }

    public class DelayTaskLifetime : IDelayTaskLifetime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly ILogger<DelayTaskLifetime> _logger;

        public DelayTaskLifetime(ILogger<DelayTaskLifetime> logger)
        {
            _logger = logger;

            ScheduleTaskStarted.Register(() =>
            {
                _logger.LogInformation("Aix.RedisDelayTask延迟任务开始执行......");
            });

            ScheduleTaskStopping.Register(() =>
            {
                _logger.LogInformation("Aix.RedisDelayTask延迟任务结束中......");
            });

            ScheduleTaskStopped.Register(() =>
            {
                _logger.LogInformation("Aix.RedisDelayTask延迟任务已结束......");
            });
        }

        public CancellationToken ScheduleTaskStarted => _startedSource.Token;

        public CancellationToken ScheduleTaskStopping => _stoppingSource.Token;

        public CancellationToken ScheduleTaskStopped => _stoppedSource.Token;

        /// <summary>
        /// 主动停止  表示开始停止
        /// </summary>
        public void Stop()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppingSource);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An error occurred stopping the Aix.RedisDelayTask", ex);
                }
            }
        }

        /// <summary>
        /// 开始时 调用通知
        /// </summary>
        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred starting the Aix.RedisDelayTask", ex);
            }
        }

        /// <summary>
        /// 结束后调用通知
        /// </summary>
        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred stopping the Aix.RedisDelayTask", ex);
            }
        }

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            // Noop if this is already cancelled
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            // Run the cancellation token callbacks
            cancel.Cancel(throwOnFirstException: false);
        }

    }
}
