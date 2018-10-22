using SampleRxLogAnalyserLib;
using SampleRxLogAnalyserLib.SampleData;
using System;
using System.Reactive.Linq;

namespace SampleRxLogAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //var logmessages = System.reaIObservable<>

            var messages = new LogQueue();
            var messageCreator = new SampleLogMessageCreator();

            // Subscribe Queue 
            messages.MessagesObserver
                .Where(log => log.AppId == messageCreator.App1)

                //                .AggregateGroupBy(log => log.ActivityId)
                //                .GroupBy(log => log.ActivityId)

                // https://stackoverflow.com/questions/41577134/rx-groupbyuntil-with-sliding-until
                .GroupByUntil(log => log.ActivityId, 
                    log => log,
                    group => group
                        .Select(message => Observable.Timer(new TimeSpan(0,0,5)))
                        .Switch()
                )
                //                .SelectMany(grp => grp.Count())
                .Select(grp => new
                {
                    ActivityId = grp.Key,
                    AnzahlMeldungen = grp.Where(item => item.LogMessage.StartsWith("Prozess info")).Count().Take(1),
                    StartDatetime = grp.Take(1),
                    MinDatetime = grp.Min(item => item.DateTime),
                    MaxDatetime = grp.Max(item => item.DateTime)
                }
                )
                .Subscribe(item =>
                    Console.WriteLine(string.Format("ActivityId: {0}, Anzahl Msg: {1}, Start: {2}, Ende: {3}",  
                        item.ActivityId, item.AnzahlMeldungen, item.MinDatetime, item.MaxDatetime)));
/*                
                .Subscribe(grp =>  Console.WriteLine("Hallo " + grp.Key + " " + grp.Count()));
*/

/*
                .Subscribe(value => 
                    Console.WriteLine(String.Format("App: {0}, Activity: {1}, DateTime: {2}: {3}", 
                                value.AppId, value.ActivityId, value.DateTime, value.LogMessage)));
*/
            messageCreator.CreateSimpleSampleData(messages);

            Console.ReadLine();

        }
    }
}
