using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Common.Reporting
{
    public class CpuUsage
    {
        public static DateTime StartTime = DateTime.UtcNow;
        private static DateTime _lastMonitorTime = DateTime.UtcNow;
        private static TimeSpan _oldCpuTime = new TimeSpan(0);
        private static TimeSpan _start;

        //private double LastMinute { get; set; }

        public double Total { get; private set; }

        public void Usage()
        {
            var newCpuTime = Process.GetCurrentProcess().TotalProcessorTime - _start;
            //LastMinute = (newCpuTime - _oldCpuTime).TotalSeconds /
            //                     (Environment.ProcessorCount * DateTime.UtcNow.Subtract(_lastMonitorTime).TotalSeconds);
            _lastMonitorTime = DateTime.UtcNow;
            Total = newCpuTime.TotalSeconds / (Environment.ProcessorCount * DateTime.UtcNow.Subtract(StartTime).TotalSeconds);
            _oldCpuTime = newCpuTime;
        }

        public void Start()
        {
            _start = Process.GetCurrentProcess().TotalProcessorTime;
        }
    }
}
