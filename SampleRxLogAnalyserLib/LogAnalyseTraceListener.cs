using SampleRxLogAnalyserLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SampleRxLogAnalyserLib
{
    public class LogAnalyseTraceListener : TraceListener
    {
        public readonly Guid AppId;
        public readonly Guid ActivityId; 
        private LogQueue log;

        public LogAnalyseTraceListener(Guid appId, Guid activityId)
        {
            AppId = appId;
            ActivityId = activityId;
            log = new LogQueue();
        }

        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string message)
        {
            log.AddMessage(AppId, ActivityId, message);
        }

        public void Quit()
        {
            log.MessagerActor.OnCompleted();
        } 

        private class OutputItem
        {
            public string FormatString { get; set; }
            public object[] Args { get; set; }
        }

        private List<OutputItem> outputs = new List<OutputItem>();


        public void Output(string formatString, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(string.Format(formatString, args));
            Console.WriteLine(string.Format("##: " + formatString, args));
            outputs.Add(new OutputItem() { FormatString = formatString, Args = args });
        }

        public IObservable<LogMessageModel> Messages {
            get
            {
                return log.MessagesObserver;
            }
        } 

    }
}
