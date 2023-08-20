



using App.Metrics.Counter;
using App.Metrics;
using CPUT.Polyglot.NoSql.Console.Extensions;
using CPUT.Polyglot.NoSql.Interface.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CPUT.Polyglot.NoSql.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dataload = false;

            var host = CreateHostBuilder(args).Build();

            host.RunAsync();

            var services = host.Services;
            var serviceScope = services.CreateScope();
            var provider = serviceScope.ServiceProvider;

            System.Console.WriteLine("Host created...");

            //get logic interface
            var serviceLogic = provider.GetRequiredService<IServiceLogic>();

            var executionBuilder = new ExecutionBuilder(serviceLogic);

            //setup data
            if (dataload)
                executionBuilder.Setup(); 

            //create test scenarios
            executionBuilder.Create();

            //execute tests
            executionBuilder.Run();

            //var metrics = provider.GetRequiredService<IMetrics>();
            //var counter = new CounterOptions { Name = "my_counter" };
            
            //metrics.Measure.Counter.Increment(counter);

            //var s = metrics.Snapshot;

            //System.Console.ReadKey();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>(); 
    }
}