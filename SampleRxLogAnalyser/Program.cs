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
            // www.introtorx.com

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
                        .Select(message => Observable.Timer(new TimeSpan(0, 0, 5)))
                        .Switch()
                )
                .Select(grp => new
                {
                    ActivityId = grp.Key,
                    AnzahlMeldungen = grp.Where(item => item.LogMessage.StartsWith("Prozess info")).Count(),
                    //MinDatetime = grp.Min(item => item.DateTime),
                    //MaxDatetime = grp.Max(item => item.DateTime)
                })
                .Select(x => Observable.FromAsync(async () => new
                {
                    ActivityId = x.ActivityId,
                    AnzahlMeldungen = await x.AnzahlMeldungen,
                    //MinDatetime = await x.MinDatetime,
                    //MaxDatetime = await x.MaxDatetime
                }))
                .Concat()

                .Subscribe(item =>
                    Console.WriteLine(string.Format("ActivityId: {0}, Anzahl Msg: {1}, ", //Start: {2}, Ende: {3}",
                        item.ActivityId, item.AnzahlMeldungen))); //, item.MinDatetime, item.MaxDatetime)));


            messageCreator.CreateSimpleSampleData(messages);

            Console.ReadLine();

        }
    }
}
