using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.RedisDelayTask
{
  internal  class Helper
    {
        static List<string> DelayTopicList = null;
        static int DelayTaskIndex = 0;

        public static string GetJobHashId(RedisDelayTaskOptions options, string id)
        {
            return $"{options.TopicPrefix}taskdata:{id}";
        }


        public static string GetDelayChannel(RedisDelayTaskOptions options)
        {
            return $"{options.TopicPrefix}DelayJobChannel";
        }

        public static string GetDelayLock(RedisDelayTaskOptions options, string delayTopicName)
        { 
        return $"{options.TopicPrefix}{delayTopicName}:lock"; 
        }

        public static List<string> GetDelayTopicList(RedisDelayTaskOptions options)
        {
            if (DelayTopicList != null) return DelayTopicList;
            DelayTopicList = new List<string>();
            //return $"{options.TopicPrefix}delay:jobid";
            for (int i = 0; i < options.DelayTopicCount; i++)
            {
                DelayTopicList.Add($"{options.TopicPrefix}delay{i}");
            }
            return DelayTopicList;
        }

       
        public static string GetDelayTopic(RedisDelayTaskOptions options, string key = null)
        {
            var count = Math.Abs(Interlocked.Increment(ref DelayTaskIndex));
            var result = GetDelayTopicList(options);
            if (string.IsNullOrEmpty(key))
            {
                return result[count % result.Count];
            }
            else
            {
                return result[Math.Abs(key.GetHashCode()) % result.Count];
            }
        }

        public static  int GetDelaySecond(RedisDelayTaskOptions options,int errorCount)
        {
            //errorCount = errorCount > 0 ? errorCount - 1 : errorCount;
            var retryStrategy = options.GetRetryStrategy();
            if (errorCount < retryStrategy.Length)
            {
                return retryStrategy[errorCount];
            }
            return retryStrategy[retryStrategy.Length - 1];
        }

    }
}
