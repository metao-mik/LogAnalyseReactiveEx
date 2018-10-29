using SampleRxLogAnalyserLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace SampleConsoleInputAnalyser
{
    class Program
    {
        static void Main(string[] args)
        {
            var myTrace = new LogAnalyseTraceListener(Guid.NewGuid(), Guid.NewGuid());
            //System.Diagnostics.Trace.Listeners.Add();
            CreateLogAnalyse2(myTrace);

            myTrace.WriteLine("Programmstart"); 
            Console.WriteLine("Hello!\n\n Bitte geben Sie einzelne Wörter oder Buchstaben ein \nDie eingaben werden gelogged und am ende analysiert.\n");

            var eingabe = string.Empty;

            do
            {
                eingabe = Console.ReadLine();
                myTrace.WriteLine(string.Format("Eingabe: {0}", eingabe));
            } while (eingabe != string.Empty);

            myTrace.WriteLine("Programmende");

            myTrace.Quit();
            Console.WriteLine("Ende");
            System.Threading.Thread.Sleep(1000 * 5);
        }



        /*
         *  Analyse über das Trace
         *  - Ausführungsdauer insgesamt ("Programmstart" bis "Programmende") 
         *  - Häufigste Eingabe welches Wort/Ausdruck
         *  - Wie oft ist das Wort "Test" oder der Buchstabe "A" eingegeben worden 
         *  - Längste Dauer zwischen 2 Eingabe 
         *  - Kürzeste Dauer zwischen 2 Eingabe 
         *  
         * */


        private static void CreateLogAnalyse1(LogAnalyseTraceListener myTrace)
        {
            var results = myTrace.Messages //messages.MessagesObserver
                .Where(log => log.AppId == myTrace.AppId)
                .Subscribe(
                    item => myTrace.Output("Item: {0}, {2}: {1}, {3}", item.AppId, item.LogMessage, item.DateTime, item.ActivityId),
                    err => myTrace.Output(@"Error: {0}", err.Message),
                    () => myTrace.Output("Completed!"));
        }

        private static void CreateLogAnalyse2(LogAnalyseTraceListener myTrace)
        {
            var results = myTrace.Messages //messages.MessagesObserver
                .Where(log => log.AppId == myTrace.AppId)
                .GroupByUntil(log => log.ActivityId,
                    log => log,
                    group => group
                        .Select(message => Observable.Timer(new TimeSpan(0, 0, 30)))
                        .Switch()
                )

                .Select(x => Observable.FromAsync(async () => new { x.Key, Log = await x.ToList() }))
                .Merge()

                .Select(x =>
                {
                    var startZeit = x.Log.Where(l => l.LogMessage.StartsWith("Programmstart")).FirstOrDefault().DateTime;
                    var endeZeit = x.Log.Where(l => l.LogMessage.StartsWith("Programmende")).FirstOrDefault().DateTime;

                    return new
                    {
                        //Appid = x.Log.FirstOrDefault(l => l.AppId),
                        //ActivityId = x.Log.FirstOrDefault(l => l.ActivityId),
                        Programmdauer = (endeZeit - startZeit),
                        AnzahlWortA = x.Log.Count(l => l.LogMessage == string.Format("Eingabe: {0}", "A")),
                        AnzahlWortTest = x.Log.Count(l => l.LogMessage == string.Format("Eingabe: {0}", "Test")),
                        AnzahlMeldungen = x.Log.Count,
                    };
                
                })

                .Subscribe(item =>
                    myTrace.Output("Programmdauer: {0}, AnzahlWortA: {1}, AnzahlWortTest: {2}, AnzahlMeldungen: {3}",
                        item.Programmdauer, item.AnzahlWortA, item.AnzahlWortTest, item.AnzahlMeldungen));
        }

    }
}
