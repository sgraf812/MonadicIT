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
using MonadicIT.Visual.Backbone;
using MonadicIT.Visual.Infrastructure;

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
            AnimateEllipse(0, TimeSpan.FromMilliseconds(500), message);
        }

        private void AnimateEllipse(int pathIndex, TimeSpan duration, Transmission message)
        {
            if (pathIndex >= _paths.Length) return;

            var path = _paths[pathIndex];
            var pg = path.Data as PathGeometry;

            var animation = new DoubleAnimationUsingPath
            {
                Duration = new Duration(duration),
                PathGeometry = pg,
            };

            var circ = _ellipsePool.Allocate();
            circ.Width = 10;
            circ.Height = 10;
            circ.Fill = new SolidColorBrush(Colors.Black);
            circ.RenderTransform = new TranslateTransform(-5, -5);
            circ.ToolTip = message.Symbol;

            BackgroundCanvas.Children.Add(circ);

            animation.Source = PathAnimationSource.X;
            circ.BeginAnimation(Canvas.LeftProperty, animation);

            animation.Completed += delegate
            {
                BackgroundCanvas.Children.Remove(circ);
                _ellipsePool.Free(circ);
                AnimateEllipse(pathIndex + 1, duration, message);
            };
            animation.Source = PathAnimationSource.Y;
            circ.BeginAnimation(Canvas.TopProperty, animation);
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