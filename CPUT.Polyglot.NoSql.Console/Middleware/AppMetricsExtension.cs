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
using Microsoft.Toolkit;

namespace CPUT.Polyglot.NoSql.Console.Middleware
{
    public static class AppMetricsExtension
    {
        public static IServiceCollection AddMetricsExtension(this IServiceCollection services)
        {
            var filter = new MetricsFilter()
                .WhereType(MetricType.Apdex, MetricType.Gauge, MetricType.Timer, MetricType.Counter)
                .WhereContext("Unified Query")
                .WhereTaggedWithKey(new string[] { "query" });


            var metrics = new MetricsBuilder()
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
