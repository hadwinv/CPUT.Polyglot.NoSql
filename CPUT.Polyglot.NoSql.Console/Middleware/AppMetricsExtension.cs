using App.Metrics.Filtering;
using App.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Formatters.Json;
using App.Metrics.Formatters.InfluxDB;
using App.Metrics.Timer;

namespace CPUT.Polyglot.NoSql.Console.Middleware
{
    public static class AppMetricsExtension
    {
        public static IServiceCollection AddMetricsExtension(this IServiceCollection services)
        {
            var filter = new MetricsFilter()
                .WhereType(MetricType.Apdex, MetricType.Gauge, MetricType.Timer, MetricType.Counter );
                //.WhereContext("Application")
                //.WhereNameStartsWith("test_");

            var metrics = new MetricsBuilder()
                //.Report.ToInfluxDb(
                //    options => {
                //        options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
                //        options.InfluxDb.Database = "metricsdatabase";
                //        options.InfluxDb.Consistenency = "consistency";
                //        options.InfluxDb.UserName = "admin";
                //        options.InfluxDb.Password = "adminadmin";
                //        options.InfluxDb.RetentionPolicy = "rp";
                //        options.InfluxDb.CreateDataBaseIfNotExists = true;
                //        options.HttpPolicy.BackoffPeriod = TimeSpan.FromSeconds(30);
                //        options.HttpPolicy.FailuresBeforeBackoff = 5;
                //        options.HttpPolicy.Timeout = TimeSpan.FromSeconds(10);
                //        options.MetricsOutputFormatter = new MetricsInfluxDbLineProtocolOutputFormatter();
                //        //options.Filter = filter;
                //        options.FlushInterval = TimeSpan.FromSeconds(20);
                //    })
                .Report.ToTextFile(options =>
                {
                    options.AppendMetricsToTextFile = true;
                    options.MetricsOutputFormatter = new MetricsJsonOutputFormatter();
                    options.OutputPathAndFileName = @"C:\metrics\polyglot_metrics.json";
                    options.FlushInterval = TimeSpan.FromSeconds(5);
                    options.Filter = filter;
                })
                .Build();

            services.AddMetrics(metrics);
            services.AddMetricsReportingHostedService();


            return services;

        }

    }
}
