using SampleRxLogAnalyserLib.Models;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace SampleRxLogAnalyserLib
{
    public class LogQueue
    {
        private List<LogMessageModel> messages = new List<LogMessageModel>();
        private DateTime currentDateTime;
        private TimeSpan SleptTime;

        private Subject<LogMessageModel> messagesSubject = new Subject<LogMessageModel>();

        public LogQueue()
        {
            currentDateTime = DateTime.Now;
        }

        public IObservable<LogMessageModel> MessagesObserver {
            get { return messagesSubject; }
        }

        public IObserver<LogMessageModel> MessagerActor
        {
            get { return messagesSubject; }
        }

        public void AddMessage(Guid appId, Guid activityId, string message)
        {
            AddMessage(appId, activityId, message, 0);
        }

        public void AddMessage(Guid appId, Guid activityId, string message, int secondsSleep )
        {
            var m = new LogMessageModel() { AppId = appId, ActivityId = activityId, DateTime = currentDateTime, LogMessage = message };

            messages.Add(m);
            messagesSubject.OnNext(m);
            if (secondsSleep > 0) Sleep(secondsSleep);
        }

        public void Sleep(TimeSpan time)
        {
            SleptTime += time;
        }
        public void Sleep(int seconds)
        {
            SleptTime += new TimeSpan(0, 0, seconds);
        }
    }
}
