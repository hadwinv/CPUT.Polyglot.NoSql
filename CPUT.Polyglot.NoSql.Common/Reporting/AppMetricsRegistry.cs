using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;
using Unit = App.Metrics.Unit;

namespace CPUT.Polyglot.NoSql.Common.Reporting
{
    public static class AppMetricsRegistry
    {
        public static class ApdexScores
        {
            private static readonly string Context = "Polyglot NoSQL System";

            public static ApdexOptions PolyglotNoSqlApdex = new ApdexOptions
            {
                Name = Context,
                ApdexTSeconds = 0.5,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static HistogramOptions AppHistogram => new HistogramOptions
            {
                Name = Context,
                Reservoir = () => new DefaultAlgorithmRReservoir(),
                MeasurementUnit = Unit.MegaBytes
            };

            public static GaugeOptions SystemErrors => new GaugeOptions
            {
                Name = "System Errors",
                MeasurementUnit = Unit.None
            };

            public static GaugeOptions ParserErrors => new GaugeOptions
            {
                Name = "Parser Errors",
                MeasurementUnit = Unit.None
            };

            public static GaugeOptions TranslatorErrors => new GaugeOptions
            {
                Name = "Translator Errors",
                MeasurementUnit = Unit.None
            };

            public static GaugeOptions DatabaseErrors => new GaugeOptions
            {
                Name = "Database Errors",
                MeasurementUnit = Unit.None
            };
        }

        public static class MemoryMetrics
        {
            public static GaugeOptions ApplicationMemoryGauge = new GaugeOptions
            {
                Name = "Process Physical Memory (MB)",
                MeasurementUnit = Unit.MegaBytes
            };

            public static GaugeOptions ParserMemoryGauge = new GaugeOptions
            {
                Name = "Process Physical Memory (MB)",
                MeasurementUnit = Unit.MegaBytes
            };

            public static GaugeOptions TranslatorMemoryGauge = new GaugeOptions
            {
                Name = "Process Physical Memory (MB)",
                MeasurementUnit = Unit.MegaBytes
            };

            public static GaugeOptions DatabaseMemoryGauge = new GaugeOptions
            {
                Name = "Process Physical Memory (MB)",
                MeasurementUnit = Unit.MegaBytes
            };
        }

        public static class CpuMetrics
        {
            private static readonly string Context = "Process";

            public static GaugeOptions CpuUsageTotal = new GaugeOptions
            {
                Context = Context,
                Name = "Process Total CPU Usage",
                MeasurementUnit = Unit.Percent,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessPagedMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "PagedProcess Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessPeekPagedMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "Process Peek Paged Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessPeekVirtualMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "Process Peek Paged Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessPeekWorkingSetSizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "Process Working Set",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessPrivateMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "Process Private Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions ProcessVirtualMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "Process Virtual Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions SystemNonPagedMemoryGauge = new GaugeOptions
            {
                Context = Context,
                Name = "System Non-Paged Memory",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };

            public static GaugeOptions SystemPagedMemorySizeGauge = new GaugeOptions
            {
                Context = Context,
                Name = "PagedSystem Memory Size",
                MeasurementUnit = Unit.Bytes,
                Tags = new MetricTags("reporter", "influxdb")
            };
        }

        public static class PerformanceMetrics
        {
            public static TimerOptions ParserTimer = new TimerOptions
            {
                Name = "Database Query Timer",
                MeasurementUnit = Unit.Calls,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions TranslatorTimer = new TimerOptions
            {
                Name = "Database Query Timer",
                MeasurementUnit = Unit.Calls,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions DatabaseTimer = new TimerOptions
            {
                Name = "Database Query Timer",
                MeasurementUnit = Unit.Calls,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions ResultsTimer = new TimerOptions
            {
                Name = "Database Query Timer",
                MeasurementUnit = Unit.Calls,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };
        }
    }
}
