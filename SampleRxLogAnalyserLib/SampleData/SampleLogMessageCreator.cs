using System;
using System.Collections.Generic;
using System.Text;

namespace SampleRxLogAnalyserLib.SampleData
{
    public class SampleLogMessageCreator
    {
        public Guid App1 { get; set; }
        public Guid App1ActivityId1 { get; set; }
        public Guid App1ActivityId2 { get; set; }
        public Guid App2 { get; set; }
        public Guid App2ActivityId1 { get; set; }

        public SampleLogMessageCreator()
        {
            App1 = Guid.NewGuid();
            App1ActivityId1 = Guid.NewGuid();
            App1ActivityId2 = Guid.NewGuid();

            App2 = Guid.NewGuid();
            App2ActivityId1 = Guid.NewGuid();
        }


        public void CreateSimpleSampleData(LogQueue log)
        {
            log.AddMessage(App2, App2ActivityId1, "irgendeine Message für 2");
            log.AddMessage(App1, App1ActivityId1, "Start Process");
            log.AddMessage(App2, App2ActivityId1, "irgendeine anderen Message für 2");
            log.AddMessage(App2, App2ActivityId1, "noch irgendeine andere Message für 2");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 1");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 2");
            log.AddMessage(App2, App2ActivityId1, "noch irgendeine andere Message für 2");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 3");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 4");
            log.AddMessage(App2, App2ActivityId1, "noch irgendeine andere Message für 2");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 5");
            log.AddMessage(App1, App1ActivityId1, "Prozess info 6");
            log.AddMessage(App2, App2ActivityId1, "noch irgendeine andere Message für 2");
            log.AddMessage(App1, App1ActivityId1, "Prozess beendet");
            log.AddMessage(App2, App2ActivityId1, "noch irgendeine andere Message für 2");
            log.AddMessage(App1, App1ActivityId2, "Start Process");
            log.AddMessage(App1, App1ActivityId2, "Prozess info 1");
            log.AddMessage(App1, App1ActivityId2, "Prozess info 2");
            log.AddMessage(App1, App1ActivityId2, "Prozess beendet");

        }
    }
}
