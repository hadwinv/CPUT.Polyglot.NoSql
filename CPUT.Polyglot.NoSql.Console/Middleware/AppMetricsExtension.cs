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
using App.Metrics.Formatters;

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
                .MetricFields.Configure(
                fields =>
                {
                    fields.BucketHistogram.Exclude();
                    fields.Histogram.Exclude( HistogramFields.P75, HistogramFields.P95,HistogramFields.P98, HistogramFields.P99);
                })
                .Report.ToTextFile(options =>
                {
                    options.AppendMetricsToTextFile = false;
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

    public class CustomOutputFormatter : IMetricsOutputFormatter
    {
        public MetricsMediaTypeValue MediaType => new MetricsMediaTypeValue("text", "vnd.custom.metrics", "v1", "plain");

        public MetricFields MetricFields { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task WriteAsync(Stream output,
            MetricsDataValueSource snapshot,
            CancellationToken cancellationToken = default)
        {
            // TODO: Serialize the snapshot

            return Task.CompletedTask;
        }
    }
}
