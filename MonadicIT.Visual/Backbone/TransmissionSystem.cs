using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using MonadicIT.Common;

namespace MonadicIT.Visual.Backbone
{
    public class TransmissionSystem
    {
        private static readonly TimeSpan TooLongToCare = TimeSpan.FromDays(200000);
        private readonly IObservable<Transmission> _symbols; 

        public TimeSpan NodeDelay { get { return TimeSpan.FromMilliseconds(100); } }

        public TransmissionSystem(ISourceProperties source, IEntropyCoderProperties entropyCoder, 
            IChannelCoderProperties channelCoder, IChannelProperties channel, IEventAggregator events)
        {
            var lastSample = new BehaviorSubject<DateTimeOffset>(DateTimeOffset.Now);
            var intervals = source.SymbolRate.Select(r => r > 0 ? TimeSpan.FromSeconds(1.0/r) : TooLongToCare);;
            var tickStreams = from i in intervals
                              from prev in lastSample.Take(1)
                              let next = prev + i
                              let now = DateTimeOffset.Now
                              select next <= now
                                  ? Observable.Return(-1L).Concat(Observable.Timer(now + i, i)) // we are already behind our schedule
                                  : Observable.Timer(next, i); // we can safely schedule the next tick

            var tick = tickStreams.Switch().Do(_ => lastSample.OnNext(DateTimeOffset.Now)).Do(_ => Beep(440, 10));

            _symbols = from _ in tick
                       from d in source.Distribution.Take(1)
                       from entEnc in entropyCoder.Encoder.Take(1)
                       from entDec in entropyCoder.Decoder.Take(1)
                       from chanCoder in channelCoder.Coder.Take(1)
                       from chan in channel.Channel.Take(1)
                       let symbol = d.Sample()
                       let entBits = entEnc(new[] {symbol})
                       let chanBits = chanCoder.Encode(entBits)
                       let distChanBits = chanBits.Select(b => chan.GetTransitionDistribution(b).Sample())
                       let distEntBits = chanCoder.Decode(distChanBits).Take(entBits.Count())
                       let distSymbol = Catch.ToOption(() => entDec(distEntBits).First())
                       select new Transmission
                       {
                           Symbol = symbol,
                           EntropyBits = entBits,
                           ChannelBits = chanBits,
                           DistortedChannelBits = distChanBits,
                           DistortedEntropyBits = distEntBits,
                           DistortedSymbol = distSymbol
                       };

            _symbols.Subscribe(events.PublishOnUIThread);
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool Beep(int freq, int dur);
    }
}