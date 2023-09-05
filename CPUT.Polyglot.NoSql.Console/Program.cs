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
            var runttests = true;

            var host = CreateHostBuilder(args).Build();
            
            System.Console.WriteLine("Host created...");

            host.RunAsync();

            var services = host.Services;
            var serviceScope = services.CreateScope();
            var provider = serviceScope.ServiceProvider;

           //get logic interface
            var serviceLogic = provider.GetRequiredService<IServiceLogic>();

            var executionBuilder = new ExecutionBuilder(serviceLogic);

            //setup data
            if (dataload)
                executionBuilder.Setup();

            if(runttests)
            {
                //create test scenarios
                executionBuilder.Create();

                //execute tests
                executionBuilder.Run();
            }

            System.Console.ReadKey();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>(); 
    }
}