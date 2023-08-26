using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;
using Microsoft.Win32;
using static MongoDB.Driver.WriteConcern;
using Unit = App.Metrics.Unit;

namespace CPUT.Polyglot.NoSql.Common.Reporting
{
    public static class MetricsRegistry
    {
        public static string Context = "Unified Query";

        public static void Tag(string value)
        {
            Apdex.Query.Tags = new MetricTags("query", value);

            CPU.Usage.Tags = new MetricTags("query", value);
            Memory.VirtualSize.Tags = new MetricTags("query", value);
            Memory.PhysicalSize.Tags = new MetricTags("query", value);

            Calls.Parser.Tags = new MetricTags("query", value);
            Calls.Translator.Tags = new MetricTags("query", value);
            Calls.Executor.Tags = new MetricTags("query", value);

            Errors.General.Tags = new MetricTags("query", value);
            Errors.Parser.Tags = new MetricTags("query", value);
            Errors.Translator.Tags = new MetricTags("query", value);
            Errors.Executor.Tags = new MetricTags("query", value);
        }
      
        public static void Reset()
        {
            Apdex.Query.ResetOnReporting = true;
            CPU.Usage.ResetOnReporting = true;
            Memory.VirtualSize.ResetOnReporting = true;
            Memory.PhysicalSize.ResetOnReporting = true;

            Calls.Parser.ResetOnReporting = true;
            Calls.Translator.ResetOnReporting = true;
            Calls.Executor.ResetOnReporting = true;

            Errors.General.ResetOnReporting = true;
            Errors.Parser.ResetOnReporting = true;
            Errors.Translator.ResetOnReporting = true;
            Errors.Executor.ResetOnReporting = true;
        }

        public static class Apdex
        {
            public static ApdexOptions Query = new ApdexOptions
            {
                Context = Context,
                Name = "ApdexScore",
                ApdexTSeconds = 2
            };
        }

        public static class CPU
        {
            public static GaugeOptions Usage { get; } = new GaugeOptions
            {
                Context = Context,
                Name = "CPU Utilisation(%)",
                MeasurementUnit = Unit.Percent
            };
        }

        public static class Memory
        {
            public static GaugeOptions PhysicalSize { get; } = new GaugeOptions
            {
                Context = Context,
                Name = "Physical Memory",
                MeasurementUnit = Unit.MegaBytes
            };

            public static GaugeOptions VirtualSize = new GaugeOptions
            {
                Context = Context,
                Name = "Private Memory Size",
                MeasurementUnit = Unit.MegaBytes
            };
        }

        public static class Calls
        {
            public static TimerOptions Parser = new TimerOptions
            {
                Context = Context,
                Name = "Global Query Parser",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions Translator = new TimerOptions
            {
                Context = Context,
                Name = "Query Translator",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions Executor = new TimerOptions
            {
                Context = Context,
                Name = "Native Query Executor",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds
            };
        }

        public static class Errors
        {
           
            public static CounterOptions General = new CounterOptions
            {
                Context = Context,
                Name = "General Errors",
                MeasurementUnit = Unit.Calls
            };

            public static CounterOptions Parser = new CounterOptions
            {
                Context = Context,
                Name = "Parser Errors",
                MeasurementUnit = Unit.Calls
            };

            public static CounterOptions Translator = new CounterOptions
            {
                Context = Context,
                Name = "Translator Errors",
                MeasurementUnit = Unit.Calls
            };

            public static CounterOptions Executor = new CounterOptions
            {
                Context = Context,
                Name = "Native Executor Errors",
                MeasurementUnit = Unit.Calls
            };
        }
    }
}
