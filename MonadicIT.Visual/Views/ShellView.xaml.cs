using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Caliburn.Micro;
using Codeplex.Reactive;
using MonadicIT.Common;
using MonadicIT.Visual.Backbone;
using MonadicIT.Visual.Infrastructure;
using Action = System.Action;

namespace MonadicIT.Visual.Views
{
    public partial class ShellView : Window, IHandle<Transmission>
    {
        private const bool IsPathStroked = true;
        private static readonly IEqualityComparer<IEnumerable<Point>> PointsComparer = new EnumerableComparer<Point>();
        private readonly Path[] _paths;
        private readonly ObjectPool<Ellipse> _ellipsePool = new ObjectPool<Ellipse>(); 
        private FrameworkElement _cdec;
        private FrameworkElement _cenc;
        private FrameworkElement _channel;
        private FrameworkElement _edec;
        private FrameworkElement _eenc;
        private FrameworkElement _sink;
        private FrameworkElement _source;
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(1000);

        public ShellView()
        {
            InitializeComponent();
            _paths = new[]
            {
                SourceToEntropyEncoder, EntropyEncoderToChannelEncoder, ChannelEncoderToChannelTop,
                ChannelTopToChannelBottom, ChannelBottomToChannelDecoder, ChannelDecoderToEntropyDecoder,
                EntropyDecoderToSink
            };

            IoC.Get<IEventAggregator>().Subscribe(this);

            Loaded += delegate
            {
                // we do it just for side effect of binding the pathgeometry properties
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                PathsFollowingElements().ObserveElements().Zip(_paths, BindPathGeometry).Count();
            };
        }

        public void Handle(Transmission message)
        {
            AnimateSourceToEntropyEncoder(message);
        }

        private void AnimateSourceToEntropyEncoder(Transmission transmission)
        {
            var animation = CreateAnimation(SourceToEntropyEncoder, TimeSpan.Zero, AnimationDuration);
            var circle = CreateCircle(Colors.Black, 5);
            circle.RenderTransform = new TranslateTransform(-5, -5);
            BeginAnimation(animation, circle, () =>
            {
                _ellipsePool.Free(circle);
                AnimateEntropyEncoderToChannelEncoder(transmission);
            });
        }

        private void AnimateEntropyEncoderToChannelEncoder(Transmission transmission)
        {
            var animation = CreateAnimation(EntropyEncoderToChannelEncoder, TimeSpan.Zero, AnimationDuration);
            var bitPack = CreateBitPack(transmission.EntropyBits.Select(b => b == Binary.I), 8);
            BeginAnimation(animation, bitPack, () =>
            {
                foreach (var ellipse in bitPack.Children.OfType<Ellipse>())
                    _ellipsePool.Free(ellipse);
                bitPack.Children.Clear();
                AnimateChannelEncoderToChannelTop(transmission);
            });
        }

        private void AnimateChannelEncoderToChannelTop(Transmission transmission)
        {
            var animation = CreateAnimation(ChannelEncoderToChannelTop, TimeSpan.Zero, AnimationDuration);
            var bitPack = CreateBitPack(transmission.ChannelBits.Select(b => b == Binary.I), 6);
            BeginAnimation(animation, bitPack, () =>
            {
                foreach (var ellipse in bitPack.Children.OfType<Ellipse>())
                    _ellipsePool.Free(ellipse);
                bitPack.Children.Clear();
                AnimateChannelBottomToChannelDecoder(transmission);
            });
        }

        private void AnimateChannelBottomToChannelDecoder(Transmission transmission)
        {
            var animation = CreateAnimation(ChannelBottomToChannelDecoder, TimeSpan.Zero, AnimationDuration);
            var errors = transmission.ChannelBits.Zip(transmission.DistortedChannelBits, (a, b) => a != b);
            var bitPack = CreateBitPack(transmission.DistortedChannelBits.Select(b => b == Binary.I), 6, errors);
            BeginAnimation(animation, bitPack, () =>
            {
                foreach (var ellipse in bitPack.Children.OfType<Ellipse>())
                    _ellipsePool.Free(ellipse);
                bitPack.Children.Clear();
                AnimateChannelDecoderToEntropyDecoder(transmission);
            });
        }

        private void AnimateChannelDecoderToEntropyDecoder(Transmission transmission)
        {
            var animation = CreateAnimation(ChannelDecoderToEntropyDecoder, TimeSpan.Zero, AnimationDuration);
            var errors = transmission.EntropyBits.Zip(transmission.DistortedEntropyBits, (a, b) => a != b);
            var bitPack = CreateBitPack(transmission.EntropyBits.Select(b => b == Binary.I), 8, errors);
            BeginAnimation(animation, bitPack, () =>
            {
                foreach (var ellipse in bitPack.Children.OfType<Ellipse>())
                    _ellipsePool.Free(ellipse);
                bitPack.Children.Clear();
                AnimateEntropyDecoderToSink(transmission);
            });
        }

        private void AnimateEntropyDecoderToSink(Transmission transmission)
        {
            var animation = CreateAnimation(EntropyDecoderToSink, TimeSpan.Zero, AnimationDuration);
            var inner = CreateCircle(Colors.Black, 3);
            var equal = transmission.DistortedSymbol.Map(s => transmission.Symbol.Equals(s)).GetOrElse(false);
            var outer = CreateCircle(equal ? Colors.Black : Colors.Red, 5);
            var canvas = new Canvas();
            canvas.Children.Add(outer);
            canvas.Children.Add(inner);
            inner.SetValue(Canvas.LeftProperty, 2.0);
            inner.SetValue(Canvas.TopProperty, 2.0);
            outer.SetValue(Canvas.LeftProperty, 0.0);
            outer.SetValue(Canvas.TopProperty, 0.0);
            canvas.RenderTransform = new TranslateTransform(-5, -5);
            BeginAnimation(animation, canvas, () =>
            {
                foreach (var ellipse in canvas.Children.OfType<Ellipse>())
                    _ellipsePool.Free(ellipse);
                canvas.Children.Clear();
            });
        }

        private Canvas CreateBitPack(IEnumerable<bool> bits, double bitWidth, IEnumerable<bool> errors = null)
        {
            var bs = bits.ToArray();
            errors = errors ?? Enumerable.Repeat(false, bs.Length);

            if (bs.Length == 0) return new Canvas();

            var stride = (int)Math.Ceiling(Math.Sqrt(bs.Length));
            var colCount = (bs.Length-1)/stride + 1;
            var slots = bs.Zip(errors, Tuple.Create).InChunksOf(stride).SelectMany((line, i) => line.Select((t, j) => new
            {
                Bit = t.Item1,
                IsFlipped = t.Item2,
                Row = j,
                Column = i
            }));

            var canvas = new Canvas();
            foreach (var slot in slots)
            {
                var fill = slot.Bit ? Colors.Yellow : Colors.Black;
                var border = slot.IsFlipped ? Colors.Red : Colors.Black;
                var outer = CreateCircle(border, bitWidth/2);
                var inner = CreateCircle(fill, bitWidth / 2 - 2);
                canvas.Children.Add(outer);
                canvas.Children.Add(inner);
                inner.SetValue(Canvas.LeftProperty, slot.Column*bitWidth+2);
                inner.SetValue(Canvas.TopProperty, slot.Row*bitWidth+2);
                outer.SetValue(Canvas.LeftProperty, slot.Column*bitWidth);
                outer.SetValue(Canvas.TopProperty, slot.Row*bitWidth);
            }
            canvas.RenderTransform = new TranslateTransform(-bitWidth*colCount/2, -bitWidth*stride/2);
            return canvas;
        }

        private void BeginAnimation(DoubleAnimationUsingPath animation, UIElement element, Action completed = null)
        {
            completed = completed ?? delegate { };
            BackgroundCanvas.Children.Add(element);

            animation.Source = PathAnimationSource.X;
            element.BeginAnimation(Canvas.LeftProperty, animation);

            animation.Completed += delegate
            {
                BackgroundCanvas.Children.Remove(element);
                completed();
            };
            animation.Source = PathAnimationSource.Y;
            element.BeginAnimation(Canvas.TopProperty, animation);
        }

        private static DoubleAnimationUsingPath CreateAnimation(Path p, TimeSpan begin, TimeSpan duration)
        {
            return new DoubleAnimationUsingPath
            {
                BeginTime = begin,
                Duration = new Duration(duration),
                PathGeometry = ((PathGeometry) p.Data)
            };
        }

        private Ellipse CreateCircle(Color fill, double radius)
        {
            var circ = new Ellipse();//_ellipsePool.Allocate();
            circ.Width = radius*2;
            circ.Height = radius*2;
            circ.Fill = new SolidColorBrush(fill);
            circ.RenderTransform = new TranslateTransform();
            circ.ApplyAnimationClock(Canvas.LeftProperty, null);
            circ.ApplyAnimationClock(Canvas.TopProperty, null);
            return circ;
        }

        private static object BindPathGeometry(IObservable<PathGeometry> pg, Path path)
        {
            path.SetBinding(Path.DataProperty, new Binding
            {
                Source = pg.ToReactiveProperty(),
                Path = new PropertyPath("Value"),
                Mode = BindingMode.OneWay
            });
            return null;
        }

        private IObservable<PathGeometry[]> PathsFollowingElements()
        {
            _source = FindNameInTemplate(SourceSink, "Top");
            _sink = FindNameInTemplate(SourceSink, "Bottom");
            _eenc = FindNameInTemplate(EntropyCoder, "Top");
            _edec = FindNameInTemplate(EntropyCoder, "Bottom");
            _cenc = FindNameInTemplate(ChannelCoder, "Top");
            _cdec = FindNameInTemplate(ChannelCoder, "Bottom");
            _channel = FindNameInTemplate(Channel, "Mid");
            const double scaleX = 3/5.0;
            const double scaleY = 1.0;
            var points = from _ in LayoutRoot.ObserveLayoutUpdates()
                         let src = CenterPosition(_source)
                         let eenc = CenterPosition(_eenc)
                         let cenc = CenterPosition(_cenc)
                         let cht = TopPosition(_channel)
                         let chb = BottomPosition(_channel)
                         let cdec = CenterPosition(_cdec)
                         let edec = CenterPosition(_edec)
                         let snk = CenterPosition(_sink)
                         select new {src, eenc, cenc, cht, chb, cdec, edec, snk};

            var distinct = points.DistinctUntilChanged(x => new[]
            {
                x.src, x.eenc, x.cenc, x.cht, x.chb, x.cdec, x.edec, x.snk
            }, PointsComparer);
            return from p in distinct
                   let diff = p.cht - p.cenc
                   let p1 = p.cenc + new Vector(diff.X*scaleX, 0)
                   let p2 = p.cht - new Vector(0, diff.Y*scaleY)
                   let p3 = p.chb + new Vector(0, diff.Y*scaleY)
                   let p4 = p.cdec + new Vector(diff.X*scaleX, 0)
                   select new[]
                   {
                       LineGeometry(p.src, p.eenc),
                       LineGeometry(p.eenc, p.cenc),
                       BezierGeometry(p.cenc, p1, p2, p.cht),
                       LineGeometry(p.cht, p.chb),
                       BezierGeometry(p.chb, p3, p4, p.cdec),
                       LineGeometry(p.cdec, p.edec),
                       LineGeometry(p.edec, p.snk)
                   };
        }

        private static PathGeometry LineGeometry(Point from, Point to)
        {
            return new PathGeometry(new[]
            {
                new PathFigure(from, new PathSegment[]
                {
                    new LineSegment(to, IsPathStroked)
                }, false)
            });
        }

        private static PathGeometry BezierGeometry(Point from, Point cp1, Point cp2, Point to)
        {
            return new PathGeometry(new[]
            {
                new PathFigure(from, new PathSegment[]
                {
                    new BezierSegment(cp1, cp2, to, IsPathStroked)
                }, false)
            });
        }

        private Point CenterPosition(FrameworkElement element)
        {
            return element.CenterRelativeTo(BackgroundCanvas);
        }

        private Point TopPosition(FrameworkElement element)
        {
            return element.TopRelativeTo(BackgroundCanvas);
        }

        private Point BottomPosition(FrameworkElement element)
        {
            return element.BottomRelativeTo(BackgroundCanvas);
        }

        private static FrameworkElement FindNameInTemplate(Control element, string name)
        {
            return (FrameworkElement) element.Template.FindName(name, element);
        }

        private class EnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
        {
            public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(IEnumerable<T> arr)
            {
                return arr.Aggregate(0, (current, t) => current ^ t.GetHashCode());
            }
        }
    }
}