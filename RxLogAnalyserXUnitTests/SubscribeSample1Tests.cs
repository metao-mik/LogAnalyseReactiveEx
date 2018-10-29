using SampleRxLogAnalyserLib;
using SampleRxLogAnalyserLib.Models;
using SampleRxLogAnalyserLib.SampleData;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace RxLogAnalyserXUnitTests
{
    public class SubscribeSample1Tests
    {
        [Fact]
        public void Test_SampleReactiveMessages()
        {
            var dataCreator = new SampleLogMessageCreator();

            var messages = new LogQueue(); //  Subject<LogMessageModel>();

            var results = messages.MessagesObserver
                    .Where(log => log.AppId == dataCreator.App1)
                    .Subscribe(
                        item => System.Diagnostics.Trace.WriteLine(string.Format("Item: {0}, {2}: {1}", item.AppId, item.LogMessage, item.ActivityId)),
                        err => System.Diagnostics.Trace.WriteLine(@"Error: {0}", err.Message),
                        () => System.Diagnostics.Trace.WriteLine("Completed!"));

            dataCreator.CreateSimpleSampleData(messages);
            messages.MessagerActor.OnCompleted();
        }

        [Fact]
        public void Test_SampleReactiveMessagesGrouped()
        {
            var dataCreator = new SampleLogMessageCreator();

            var messages = new LogQueue(); //  Subject<LogMessageModel>();

            var results = messages.MessagesObserver
                    .Where(log => log.AppId == dataCreator.App1)
                    .GroupByUntil(log => log.ActivityId,
                        log => log,
                        group => group
                            .Select(message => Observable.Timer(new TimeSpan(0, 0, 5)))
                            .Switch()
                    )
/*
                    .SelectMany(grp =>
                    {
                        grp..
                        return grp.Scan(new ,)
                    })
*/
                    .Select(grp => new { ActivityId = grp.Key, Anz = grp.Count() })
                    .Subscribe(
                        item => System.Diagnostics.Trace.WriteLine(string.Format("Activity: {0}, Anz: {1}", item.ActivityId, item.Anz)),  // mit .FirstOrDefault() blokiert
 //                       item => System.Diagnostics.Trace.WriteLine(string.Format("Item: {0}, {2}: {1}", item.Key, item.AppId, item.LogMessage, item.ActivityId)),
                        err => System.Diagnostics.Trace.WriteLine(@"Error: {0}", err.Message),
                        () => System.Diagnostics.Trace.WriteLine("Completed!"));


            dataCreator.CreateSimpleSampleData(messages);
            System.Threading.Thread.Sleep(1000 * 5);
            messages.MessagerActor.OnCompleted();
        }


        [Fact]
        public void Test_SampleData1()
        {
            var messages = new LogQueue();
            var messageCreator = new SampleLogMessageCreator();

            // Subscribe Queue 
            var groups = messages.MessagesObserver
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
                //                .SelectMany(grp => grp.Count())
                .Select(grp => new
                {
                    ActivityId = grp.Key,
                    AnzahlMeldungen = grp.Where(item => item.LogMessage.StartsWith("Prozess info")).Count().Take(1),
                    StartDatetime = grp.Take(1),
                    MinDatetime = grp.Min(item => item.DateTime),
                    MaxDatetime = grp.Max(item => item.DateTime)
                });

            groups.Subscribe(item =>
                System.Diagnostics.Trace.WriteLine(string.Format("ActivityId: {0}, Anzahl Msg: {1}, Start: {2}, Ende: {3}",
                        item.ActivityId, 
                        item.AnzahlMeldungen.Wait(), 
                        item.MinDatetime.Wait(), 
                        item.MaxDatetime.Wait())));
            /*                
                            .Subscribe(grp =>  Console.WriteLine("Hallo " + grp.Key + " " + grp.Count()));
            */

            /*
                            .Subscribe(value => 
                                Console.WriteLine(String.Format("App: {0}, Activity: {1}, DateTime: {2}: {3}", 
                                            value.AppId, value.ActivityId, value.DateTime, value.LogMessage)));
            */
            messageCreator.CreateSimpleSampleData(messages);
            System.Threading.Thread.Sleep(1000 * 10);
        }
    }
}
