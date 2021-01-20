using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTask.Foundation
{
    /// <summary>
    /// 防重检测
    /// </summary>
    internal class RepeatChecker
    {
        private volatile bool _state = false;

        public bool Check()
        {
            if (_state) return false;
            lock (this)
            {
                if (_state) return false;
                _state = true;
                return _state;
            }
        }
    }
}
