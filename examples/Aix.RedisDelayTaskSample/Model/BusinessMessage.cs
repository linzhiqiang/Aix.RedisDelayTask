using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTaskSample.Model
{
    public class BusinessMessage
    {

        public string MessageId { get; set; }

        public string Content { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
