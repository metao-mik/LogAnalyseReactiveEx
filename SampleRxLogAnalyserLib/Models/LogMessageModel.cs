using System;
using System.Collections.Generic;
using System.Text;

namespace SampleRxLogAnalyserLib.Models
{
    public class LogMessageModel
    {
        public Guid AppId { get; set; }
        public Guid ActivityId { get; set; }
        public DateTime DateTime { get; set; }
        public String LogMessage { get; set; }

    }
}
