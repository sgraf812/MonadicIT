using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
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
        private const bool IsPathStroked = false;
        private static readonly IEqualityComparer<IEnumerable<Point>> PointsComparer = new EnumerableComparer<Point>(); 
        private FrameworkElement _source;
        private FrameworkElement _sink;
        private FrameworkElement _eenc;
        private FrameworkElement _edec;
        private FrameworkElement _cenc;
        private FrameworkElement _cdec;
        private FrameworkElement _channel;

        public ShellView()
        {
            InitializeComponent();
            IoC.Get<IEventAggregator>().Subscribe(this);
            Loaded += delegate
            {
                _source = FindNameInTemplate(SourceSink, "Top");
                _sink = FindNameInTemplate(SourceSink, "Bottom");
                _eenc = FindNameInTemplate(EntropyCoder, "Top");
                _edec = FindNameInTemplate(EntropyCoder, "Bottom");
                _cenc = FindNameInTemplate(ChannelCoder, "Top");
                _cdec = FindNameInTemplate(ChannelCoder, "Bottom");
                _channel = (FrameworkElement) FindName("Channel");

                Path.SetBinding(Path.DataProperty, new Binding
                {
                    Source = PathsFollowingElements().ToReactiveProperty(),
                    Path = new PropertyPath("Value"),
                    Mode = BindingMode.OneWay
                });
            };
        }

        private IObservable<PathGeometry> PathsFollowingElements()
        {
            const double scaleX = 3/5.0;
            const double scaleY = 1.0;
            var points = from _ in RootPanel.ObserveLayoutUpdates()
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
                   select new PathGeometry(new[]
                   {
                       new PathFigure(p.src, new PathSegment[]
                       {
                           new LineSegment(p.eenc, IsPathStroked),
                           new LineSegment(p.cenc, IsPathStroked),
                           new LineSegment(p.cenc, IsPathStroked),
                           new BezierSegment(p1, p2, p.cht, IsPathStroked),
                           new LineSegment(p.chb, IsPathStroked),
                           new BezierSegment(p3, p4, p.cdec, IsPathStroked),
                           new LineSegment(p.edec, IsPathStroked),
                           new LineSegment(p.snk, IsPathStroked),
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

        public void Handle(Transmission message)
        {
            var pg = Path.Data as PathGeometry;

            var animation = new DoubleAnimationUsingPath
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(4000)),
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.2,
                PathGeometry = pg,
            };
            var circ = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Black),
                RenderTransform = new TranslateTransform(-5, -5),
                
            };

            animation.Source = PathAnimationSource.X;
            circ.BeginAnimation(Canvas.LeftProperty, animation);
            animation.Source = PathAnimationSource.Y;
            circ.BeginAnimation(Canvas.TopProperty, animation);
            BackgroundCanvas.Children.Add(circ);
            animation.Completed += delegate
            {
                BackgroundCanvas.Children.Remove(circ);
            };
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
