using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.RedisDelayTaskSample
{
    internal static class JsonUtils
    {
        public static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            if (obj is string || obj.GetType().IsValueType)
            {
                return obj.ToString();
            }
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        public static T FromJson<T>(string str)
        {
            var result = FromJson(str, typeof(T));
            if (result == null) return default(T);
            return (T)result;
        }

        public static object FromJson(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str) || type == null)
            {
                return null;
            }

            if (type == typeof(string))
            {
                return str;
            }
            if (type.IsValueType)
            {
                return Convert.ChangeType(str, type);
            }

            return System.Text.Json.JsonSerializer.Deserialize(str, type);
        }

    }
}
