using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTask
{
  public  class DelayTaskExInfo
    {
        /// <summary>
        /// 任务id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///任务分类 一般固定一个字符串即可
        /// </summary>
        public string TaskGroup { get; set; }
    }
}
