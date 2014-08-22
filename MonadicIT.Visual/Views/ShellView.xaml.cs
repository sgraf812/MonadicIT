using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Caliburn.Micro;
using MonadicIT.Visual.Backbone;
using MonadicIT.Visual.Controls;
using MonadicIT.Visual.Infrastructure;
using Action = System.Action;

namespace MonadicIT.Visual.Views
{
    public partial class ShellView : Window, IHandle<Transmission>
    {
        private const bool IsPathStroked = true;
        private FrameworkElement _source;
        private FrameworkElement _sink;
        private FrameworkElement _eenc;
        private FrameworkElement _edec;
        private FrameworkElement _cenc;
        private FrameworkElement _cdec;
        private FrameworkElement _channel;
        private LineSegment _toEenc;
        private LineSegment _toCenc;
        private BezierSegment _toChannelTop;
        private LineSegment _toChannelBot;
        private BezierSegment _toCdec;
        private LineSegment _toEdec;
        private LineSegment _toSink;
        private PathGeometry _pathGeometry;

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
                _toEenc = new LineSegment { IsStroked = IsPathStroked };
                _toCenc = new LineSegment { IsStroked = IsPathStroked };
                _toChannelTop = new BezierSegment {IsStroked = IsPathStroked};
                _toChannelBot = new LineSegment { IsStroked = IsPathStroked };
                _toCdec = new BezierSegment { IsStroked = IsPathStroked };
                _toEdec = new LineSegment { IsStroked = IsPathStroked };
                _toSink = new LineSegment { IsStroked = IsPathStroked };
                _pathGeometry = new PathGeometry(new[]
                {
                    new PathFigure
                    {
                        Segments = new PathSegmentCollection
                        {
                            _toEenc,
                            _toCenc,
                            _toChannelTop,
                            _toChannelBot,
                            _toCdec,
                            _toEdec,
                            _toSink
                        }
                    }
                });

                var cenc = _cenc.CenterRelativeTo(RootPanel);
                var chanTop = _channel.TopRelativeTo(RootPanel);
                var chanBot = _channel.BottomRelativeTo(RootPanel);
                var cdec = _cdec.CenterRelativeTo(RootPanel);
                var diff = chanTop - cenc;
                const double scaleX = 3/5.0;
                const double scaleY = 1.0;
                var p1 = cenc + new Vector(diff.X*scaleX, 0);
                var p2 = chanTop - new Vector(0, diff.Y*scaleY);
                var p3 = chanBot + new Vector(0, diff.Y*scaleY);
                var p4 = cdec + new Vector(diff.X * scaleX, 0);

            var path = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = _source.CenterRelativeTo(RootPanel),
                        Segments = new PathSegmentCollection
                        {
                            new LineSegment(_eenc.CenterRelativeTo(RootPanel), true),
                            new LineSegment(_cenc.CenterRelativeTo(RootPanel), true),
                            new BezierSegment(p1, p2, chanTop, true),
                            new LineSegment(chanBot, true),
                        }
                    },
                    new PathFigure
                    {
                        StartPoint = chanBot,
                        Segments = new PathSegmentCollection{
                        new BezierSegment(p3, p4, cdec, true),
                            //new ArcSegment(_cdec.CenterRelativeTo(RootPanel), s, 0, false, SweepDirection.Clockwise, true),
                            new LineSegment(_edec.CenterRelativeTo(RootPanel), true),
                            new LineSegment(_sink.CenterRelativeTo(RootPanel), true),}
                    }
                }
            };

            var p = new Path { Data = path, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1 };
            Grid.SetColumnSpan(p, RootPanel.ColumnDefinitions.Count);
                RootPanel.Children.Insert(0, p);
            };
        }

        public void Handle(Transmission message)
        {
            var pg = (RootPanel.Children[0] as Path).Data as PathGeometry;

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
            Canv.Children.Add(circ);
            animation.Completed += delegate
            {
                Canv.Children.Remove(circ);
            };
        }

        private static FrameworkElement FindNameInTemplate(Control element, string name)
        {
            return (FrameworkElement) element.Template.FindName(name, element);
        }
    }
}
