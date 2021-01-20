using Aix.RedisDelayTaskSample.Model;
using CommandLine;
using Microsoft.Extensions.Hosting;
using System;

namespace Aix.RedisDelayTaskSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser((setting) =>
            {
                setting.CaseSensitive = false;
            });

            parser.ParseArguments<CmdOptions>(args).WithParsed((options) =>
            {
                CmdOptions.Options = options;
                CreateHostBuilder(args).Build().Run();
            });
            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
               .ConfigureHostConfiguration(configurationBuilder =>
               {
               })
              .ConfigureAppConfiguration((hostBulderContext, configurationBuilder) =>
              {

              })
               .ConfigureLogging((hostBulderContext, loggingBuilder) =>
               {

               })
               .ConfigureServices(Startup.ConfigureServices);
        }
    }
}
