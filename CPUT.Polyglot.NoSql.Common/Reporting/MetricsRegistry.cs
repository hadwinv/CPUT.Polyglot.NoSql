using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;
using Unit = App.Metrics.Unit;

namespace CPUT.Polyglot.NoSql.Common.Reporting
{
    public static class MetricsRegistry
    {
        public static class Apdex
        {
            private static readonly string Context = "Apdex Score";
            
            public static ApdexOptions Query = new ApdexOptions
            {
                Context = Context,
                Name = "Query",
                ApdexTSeconds = 0.5,
                Tags = new MetricTags("global", "query")
            };
        }

        public static class CPU
        {
            private static readonly string Context = "CPU";

            public static GaugeOptions Usage { get; } = new GaugeOptions
            {
                Context = Context,
                Name = "Utilisation(%)",
                MeasurementUnit = Unit.Percent,
                Tags = new MetricTags("global", "query")
            };
        }

        public static class Memory
        {
            //https://stackoverflow.com/questions/1984186/what-is-private-bytes-virtual-bytes-working-set
            private static readonly string Context = "Process";

            public static GaugeOptions PhysicalSize { get; } = new GaugeOptions
            {
                Context = Context,
                Name = "Physical Memory",
                MeasurementUnit = Unit.KiloBytes,
                Tags = new MetricTags("global", "query")
            };

            public static GaugeOptions VirtualSize = new GaugeOptions
            {
                Context = Context,
                Name = "Private Memory Size",
                MeasurementUnit = Unit.KiloBytes,
                Tags = new MetricTags("global", "query")
            };
        }

        public static class Calls
        {
            private static readonly string Context = "Execution Time";

            public static TimerOptions Parser = new TimerOptions
            {
                Context = Context,
                Name = "Global Query Parser",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                Tags = new MetricTags("query", "parser")
            };

            public static TimerOptions Translator = new TimerOptions
            {
                Context = Context,
                Name = "Query Translator",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds,
                //RateUnit = TimeUnit.Milliseconds,
                Tags = new MetricTags("query", "translator")
            };
            

            public static TimerOptions Executor = new TimerOptions
            {
                Context = Context,
                Name = "Native Query Executor",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds,
                Tags = new MetricTags("query", "native execution")
            };
        }

        public static class Errors
        {
           
            public static CounterOptions General = new CounterOptions
            {
                Name = "General Errors",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("global", "errors")
            };

            public static CounterOptions Parser = new CounterOptions
            {
                Name = "Global Parser Errors",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("query", "parser")
            };

            public static CounterOptions Translator = new CounterOptions
            {
                Name = "Query Translator Errors",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("query", "translator")
            };

            public static CounterOptions Executor = new CounterOptions
            {
                Name = "Native Query Executor Errors",
                MeasurementUnit = Unit.Calls,
                Tags = new MetricTags("query", "native execution")
            };
        }
    }
}
