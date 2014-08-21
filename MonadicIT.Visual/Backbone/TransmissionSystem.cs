using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace MonadicIT.Visual.Backbone
{
    public class TransmissionSystem
    {
        private static readonly TimeSpan TooLongToCare = TimeSpan.FromDays(200000);
        private readonly IObservable<object> _symbols;

        public TransmissionSystem(ISourceProperties source, IEntropyCoderProperties entropyCoder, 
            IChannelCoderProperties channelCoder, IChannelProperties channel)
        {
            var rate = source.SymbolRate;
            var lastSample = new BehaviorSubject<DateTimeOffset>(DateTimeOffset.Now);
            var intervals = rate.Select(r => r > 0 ? TimeSpan.FromSeconds(1.0/r) : TooLongToCare);;
            var bla = from i in intervals
                      let next = lastSample.Value + i
                      let now = DateTimeOffset.Now
                      select next <= now
                          ? Observable.Return(-1L).Concat(Observable.Timer(now + i, i))
                          : Observable.Timer(next, i);

            var tick = bla.Switch().Do(_ => lastSample.OnNext(DateTimeOffset.Now)).Do(_ => Beep(440, 10));
            _symbols = from _ in tick
                       from d in source.Distribution.Take(1)
                       select d.Sample();

            _symbols.Subscribe(s => Trace.WriteLine(s));
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool Beep(int freq, int dur);
    }
}