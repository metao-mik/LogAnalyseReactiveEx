using SampleRxLogAnalyserLib;
using SampleRxLogAnalyserLib.Models;
using SampleRxLogAnalyserLib.SampleData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace RxLogAnalyserXUnitTests
{
    public class SubscribeSample1Tests
    {
        private class OutputItem
        {
            public string FormatString { get; set; }
            public object[] Args { get; set; }
        }

        private List<OutputItem> outputs = new List<OutputItem>();
//        private outputs = new List< { string formatString, params object[] args }>()


        [Fact]
        public void Test_SampleReactiveMessages()
        {
            var dataCreator = new SampleLogMessageCreator();

            var messages = new LogQueue(); //  Subject<LogMessageModel>();

            var results = messages.MessagesObserver
                    .Where(log => log.AppId == dataCreator.App1)
                    .Subscribe(
                        item => Output("Item: {0}, {2}: {1}, {3}", item.AppId, item.LogMessage, item.DateTime, item.ActivityId),
                        err => Output(@"Error: {0}", err.Message),
                        () => Output("Completed!"));

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
                    .Select(grp => new { ActivityId = grp.Key, Anz = grp.Count() })
                    .Subscribe(
                        async item => System.Diagnostics.Trace.WriteLine(string.Format("Activity: {0}, Anz: {1}", item.ActivityId, await item.Anz)),  // mit .FirstOrDefault() blokiert
 //                       item => System.Diagnostics.Trace.WriteLine(string.Format("Item: {0}, {2}: {1}", item.Key, item.AppId, item.LogMessage, item.ActivityId)),
                        err => System.Diagnostics.Trace.WriteLine(@"Error: {0}", err.Message),
                        () => System.Diagnostics.Trace.WriteLine("Completed!"));


            dataCreator.CreateSimpleSampleData(messages);
            System.Threading.Thread.Sleep(1000 * 5);
            messages.MessagerActor.OnCompleted();
        }


        [Fact]
        public void Test_GroupSampleData1AsyncAwait()
        {
            var messages = new LogQueue();
            var messageCreator = new SampleLogMessageCreator();

            // Subscribe Queue 
            messages.MessagesObserver
                .Where(log => log.AppId == messageCreator.App1)

                .GroupByUntil(log => log.ActivityId,
                    log => log,
                    group => group
                        .Select(message => Observable.Timer(new TimeSpan(0, 0, 3)))
                        .Switch()
                )
                .Select(grp => new
                {
                    ActivityId = grp.Key,
                    AnzahlMeldungen = grp.Where(item => item.LogMessage.StartsWith("Prozess info")).Count(),
                    //MinDatetime = grp.Min(item => item.DateTime),
                    //MaxDatetime = grp.Max(item => item.DateTime)
                })

                .Subscribe(async item =>
                    System.Diagnostics.Trace.WriteLine(string.Format("ActivityId: {0}, Anzahl Msg: {1}, ", //Start: {2}, Ende: {3}",
                        item.ActivityId, await item.AnzahlMeldungen))); //, item.MinDatetime, item.MaxDatetime)));

            messageCreator.CreateSimpleSampleData(messages);

            System.Threading.Thread.Sleep(1000 * 4);
        }


        [Fact]
        public void Test_GroupSampleData1Merge()
        {
            var messages = new LogQueue();
            var sampleDataCreator = new SampleLogMessageCreator();

            // Subscribe Queue 
            messages.MessagesObserver
                .Where(log => log.AppId == sampleDataCreator.App1)

                .GroupByUntil(log => log.ActivityId,
                    log => log,
                    group => group
                        .Select(message => Observable.Timer(new TimeSpan(0, 0, 2)))
                        .Switch()
                )

                .Select(x => Observable.FromAsync(async () => new { x.Key, Log = await x.ToList() }))
                .Merge()

                .Select(
                    x => new {
                        ActivityId = x.Key,
                        AnzahlMeldungen = x.Log.Count,
                        AnzahlMeldungenProzessInfo = x.Log.Where(item => item.LogMessage.StartsWith("Prozess info")).Count(),
                        MinDatetime = x.Log.Min(log => log.DateTime),
                        MaxDatetime = x.Log.Max(log => log.DateTime),
                    })

                .Subscribe(item =>
                    Output("ActivityId: {0}, Anzahl Msg: {1}, Start: {2}, Ende: {3}",
                        item.ActivityId, item.AnzahlMeldungen, item.MinDatetime, item.MaxDatetime));


            sampleDataCreator.CreateSimpleSampleData(messages); // Beispieldaten erstellen
            System.Threading.Thread.Sleep(1000 * 3);    // Zeit geben zum verarbeiten 


            Assert.True(outputs.Count == 2);
        }



        [Fact]
        public void Test_GroupSampleData1Merge_o001_Beispiel()
        {
            var messages = new LogQueue();
            var messageCreator = new SampleLogMessageCreator();

            // Subscribe Queue 
            messages.MessagesObserver
                .Where(log => log.AppId == messageCreator.App1)

                .GroupByUntil(log => log.ActivityId,
                    log => log,
                    group => group
                        .Select(message => Observable.Timer(new TimeSpan(0, 0, 3)))
                        .Switch()
                )

                .Select(x => Observable.FromAsync(async () => new { x.Key, Log = await x.ToList() }))
                .Merge()

                .Select(
                    x => {
                        // Beispiel o001 
                        var zeile = x.Log.Where(item => item.LogMessage.Contains("<OrderNumber>")).FirstOrDefault();
                        // var zeile2 = x.Log.WhereItemContains("<OrderNumber>");
                        // regexWunder => WunderWert 
                        var WunderWert = "Hallo";

                        return new
                        {
                            ActivityId = x.Key,
                            AnzahlMeldungen = x.Log.Count,
                            AnzahlMeldungenProzessInfo = x.Log.Where(item => item.LogMessage.StartsWith("Prozess info")).Count(),
                            MinDatetime = x.Log.Min(log => log.DateTime),
                            MaxDatetime = x.Log.Max(log => log.DateTime),
                            Auftragsnummer = WunderWert
                        };
                    })

                .Subscribe(item =>
                    Output("ActivityId: {0}, Anzahl Msg: {1}, Start: {2}, Ende: {3}",
                        item.ActivityId, item.AnzahlMeldungen, item.MinDatetime, item.MaxDatetime));

            messageCreator.CreateSimpleSampleData(messages);

            System.Threading.Thread.Sleep(1000 * 4);
        }


        private void Output(string formatString, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(string.Format(formatString, args));
            outputs.Add(new OutputItem() { FormatString = formatString, Args = args });
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
