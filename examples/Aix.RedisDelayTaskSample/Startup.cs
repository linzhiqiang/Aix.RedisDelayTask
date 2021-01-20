using Aix.RedisDelayTaskSample.HostServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Aix.RedisDelayTask;
using Aix.RedisDelayTaskSample.Model;

namespace Aix.RedisDelayTaskSample
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            #region 延迟任务相关

            services.AddRedisDelayTask(options=> {
                options.ConnectionString = "127.0.0.1:6379";
            });

            #endregion

            //测试代码
            var options = CmdOptions.Options;
            services.AddSingleton(options);
            if ((options.Mode & (int)ClientMode.Consumer) > 0)
            {
                services.AddHostedService<DelayTaskConsumerService>();
            }
            if ((options.Mode & (int)ClientMode.Producer) > 0)
            {
                services.AddHostedService<DelayTaskProduerService>();
            }
          
        }
    }
}
