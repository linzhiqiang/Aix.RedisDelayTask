using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTask
{
    public class ConsumeDelayTaskResult
    {
        /// <summary>
        /// 任务id
        /// </summary>
        public string  Id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TaskGroup { get; set; }

        /// <summary>
        /// 任务内容
        /// </summary>
        public string TaskContent { get; set; }

        /// <summary>
        /// 二进制内容
        /// </summary>
        public byte[] TaskBytesContent { get; set; }

        /// <summary>
        /// 错误重试次数  不能超过重试最大次数
        /// </summary>
        public int ErrorRetryCount { get; set; }
    }
}
