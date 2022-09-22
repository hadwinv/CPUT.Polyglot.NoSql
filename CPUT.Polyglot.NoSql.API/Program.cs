using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CPUT.Polyglot.NoSql.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //MongoClient dbClient = new MongoClient(<< YOUR ATLAS CONNECTION STRING >>);

            //var dbList = dbClient.ListDatabases().ToList();

            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{
            //    Console.WriteLine(db);
            //}

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //.UseUrls("http://*:8080")
                    //.UseSetting("https_port", "8080")
                    //.UseIIS()
                    //.UseIISIntegration()
                    //.Build();
                });
    }
    //var config = new ConfigurationBuilder()
    //        .SetBasePath(Directory.GetCurrentDirectory())
    //        .Build();

    //        return WebHost.CreateDefaultBuilder(args)
    //            .UseConfiguration(config)
    //            .UseKestrel(options => { options.Limits.KeepAliveTimeout = new System.TimeSpan(0, 5, 0); })
    //            .ConfigureLogging(logConfig =>
    //            {
    //    logConfig.ClearProviders();
    //})
    //            .ConfigureAppConfiguration((hostingContext, conf) =>
    //            {

    //    conf.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    //    conf.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
    //    conf.AddJsonFile($"version.json", optional: true, reloadOnChange: true);
    //    conf.SetBasePath(Directory.GetCurrentDirectory());
    //    conf.AddEnvironmentVariables();
    //})
    //            .UseStartup<Startup>()
    //            .UseUrls("http://*:8080")
    //            .UseSetting("https_port", "8080")
    //            .UseIIS()
    //            .UseIISIntegration()
    //            .Build();
}
