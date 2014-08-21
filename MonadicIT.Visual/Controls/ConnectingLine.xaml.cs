using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;

namespace MonadicIT.Visual.Controls
{
    public partial class ConnectingLine : UserControl
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From", typeof(FrameworkElement), typeof(ConnectingLine), new PropertyMetadata(default(FrameworkElement)));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To", typeof(FrameworkElement), typeof(ConnectingLine), new PropertyMetadata(default(FrameworkElement)));

        public FrameworkElement From
        {
            get { return (FrameworkElement) GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public FrameworkElement To
        {
            get { return (FrameworkElement) GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public ConnectingLine()
        {
            InitializeComponent();
            var froms = CenterPointsOf(x => x.From);
            var tos = CenterPointsOf(x => x.To);

            froms.Subscribe(p =>
            {
                if (double.IsNaN(p.X) || double.IsNaN(p.Y)) return;
                InnerLine.X1 = p.X;
                InnerLine.Y1 = p.Y;
            });

            tos.Subscribe(p =>
            {
                if (double.IsNaN(p.X) || double.IsNaN(p.Y)) return;
                InnerLine.X2 = p.X;
                InnerLine.Y2 = p.Y;
            });
        }

        private IObservable<Point> CenterPointsOf(Expression<Func<ConnectingLine, FrameworkElement>> expr)
        {
            var accessor = expr.Compile();

            return from e in (from f in this.ObservableForProperty(expr)
                              select Observable.FromEventPattern(
                                  h => f.Value.LayoutUpdated += h,
                                  h => f.Value.LayoutUpdated -= h)).Switch()
                   select CenterOf(accessor(this));
        }

        private Point CenterOf(FrameworkElement element)
        {
            return element.TransformToVisual(this).Transform(new Point(element.Width/2.0, element.Height/2.0));
        }
    }
}
