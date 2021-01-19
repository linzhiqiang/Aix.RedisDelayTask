using Aix.RedisDelayTask.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTask.Model
{
    public class TaskData
    {
        public static TaskData Create(string taskGroup, string taskContent, TimeSpan delay)
        {
            return new TaskData
            {
                TaskGroup = taskGroup,
                TaskContent = taskContent,
               // ExecuteTime = DateUtils.GetTimeStamp(DateTime.Now.Add(delay))
            };
        }

        public TaskData()
        {
            Id = Guid.NewGuid().ToString().Replace("-", "");
        }

        public string Id { get; set; }

        /// <summary>
        /// 任务所属组/分类
        /// </summary>
        public string TaskGroup { get; set; }

        public string TaskContent { get; set; }

        /// <summary>
        /// 执行时间  时间戳 毫秒数
        /// </summary>
       // public long ExecuteTime { get; set; }

        /// <summary>
        /// 错误重试次数  不能超过重试最大次数
        /// </summary>
        public int ErrorRetryCount { get; set; }

        public void ResetExecuteTime(TimeSpan delay)
        {
           // ExecuteTime = DateUtils.GetTimeStamp(DateTime.Now.Add(delay));
        }


        public static TaskData ToTaskData(NameValueEntry[] nameValues)
        {
            var dict = ToDictionary(nameValues);
            return ToTaskData(dict);
        }

        public NameValueEntry[] ToNameValueEntries()
        {
            var taskData = this;
            var nameValues = new NameValueEntry[] {
                 new NameValueEntry(nameof( TaskData.Id),taskData.Id?? ""),
                  new NameValueEntry(nameof( TaskData.TaskGroup),taskData.TaskGroup?? ""),
                  new NameValueEntry(nameof( TaskData.TaskContent),taskData.TaskContent?? ""),
                // new NameValueEntry(nameof( TaskData.ExecuteTime),taskData.ExecuteTime),
                  new NameValueEntry(nameof( TaskData.ErrorRetryCount),taskData.ErrorRetryCount),
                };
            return nameValues;

        }

        public HashEntry[] ToHashEntries()
        {
            var taskData = this;
            var nameValues = new HashEntry[] {
                new HashEntry(nameof( TaskData.Id),taskData.Id?? ""),
                  new HashEntry(nameof( TaskData.TaskGroup),taskData.TaskGroup?? ""),
                  new HashEntry(nameof( TaskData.TaskContent),taskData.TaskContent?? ""),
                 // new HashEntry(nameof( TaskData.ExecuteTime),taskData.ExecuteTime),
                 new HashEntry(nameof( TaskData.ErrorRetryCount),taskData.ErrorRetryCount),
                };

            return nameValues;

        }

        public static TaskData ToTaskData(HashEntry[] hashEntries)
        {
            var dict = hashEntries.ToDictionary();
            return ToTaskData(dict);
        }

        public static TaskData ToTaskData(Dictionary<RedisValue, RedisValue> keyValues)
        {
            var taskData = new TaskData
            {
                Id = keyValues.GetValue(nameof(TaskData.Id)),
                TaskGroup = keyValues.GetValue(nameof(TaskData.TaskGroup)),
                TaskContent = keyValues.GetValue(nameof(TaskData.TaskContent)),
               // ExecuteTime = NumberUtils.ToLong(keyValues.GetValue(nameof(TaskData.ExecuteTime))),
                ErrorRetryCount = NumberUtils.ToInt(keyValues.GetValue(nameof(TaskData.ErrorRetryCount))),
            };

            return taskData;

        }


        private static Dictionary<RedisValue, RedisValue> ToDictionary(NameValueEntry[] nameValueEntries)
        {
            var dict = new Dictionary<RedisValue, RedisValue>();
            if (nameValueEntries != null && nameValueEntries.Length > 0)
            {
                foreach (var item in nameValueEntries)
                {
                    dict.Add(item.Name, item.Value);
                }
            }
            return dict;

        }
    }
}
