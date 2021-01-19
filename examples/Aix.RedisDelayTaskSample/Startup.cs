using Aix.RedisDelayTaskSample.HostServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Aix.RedisDelayTask;

namespace Aix.RedisDelayTaskSample
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            #region 任务执行器相关

            services.AddRedisDelayTask(options=> {
                options.ConnectionString = "127.0.0.1:6379";
            });

            #endregion

            //入口服务
            services.AddHostedService<StartHostService>();
        }
    }
}
