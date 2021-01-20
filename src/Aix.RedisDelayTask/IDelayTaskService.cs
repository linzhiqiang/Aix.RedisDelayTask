
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTask
{
    /// <summary>
    /// 延迟任务服务
    /// </summary>
    public interface IDelayTaskService : IDisposable
    {
        /// <summary>
        ///  新增延迟任务
        /// </summary>
        /// <param name="content"></param>
        /// <param name="delay"></param>
        /// <param name="delayTaskExInfo"></param>
        /// <returns></returns>
        Task<string> PublishAsync( string content, TimeSpan delay, DelayTaskExInfo delayTaskExInfo=null);

        /// <summary>
        /// 返回true 不进行重试，返回false 进行重试，抛出异常也进行重试（建议捕获异常 决定是否重试）
        /// </summary>
        /// <param name="handler">建议异步执行（通过插入消息队列执行）</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SubscribeAsync(Func<ConsumeDelayTaskResult, Task<bool>> handler, CancellationToken cancellationToken = default);

        //Task StartConsume(CancellationToken cancellationToken = default);
    }
}
