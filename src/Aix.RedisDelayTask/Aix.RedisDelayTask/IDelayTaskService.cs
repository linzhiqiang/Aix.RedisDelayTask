
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.RedisDelayTask
{
    public interface IDelayTaskService : IDisposable
    {
        /// <summary>
        /// 新增延迟任务
        /// </summary>
        /// <param name="taskGroup">任务分类 一般固定一个字符串即可</param>
        /// <param name="content"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        Task<string> PublishDelayAsync(string taskGroup, string content, TimeSpan delay);

        /// <summary>
        /// 返回true 不进行重试，返回false 进行重试
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SubscribeAsync(Func<ConsumeDelayTaskResult, Task<bool>> handler, CancellationToken cancellationToken = default);


    }
}
