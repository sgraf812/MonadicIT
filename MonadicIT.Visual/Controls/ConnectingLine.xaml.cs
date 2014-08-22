using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Codeplex.Reactive.Extensions;
using MonadicIT.Visual.Annotations;
using MonadicIT.Visual.Infrastructure;

namespace MonadicIT.Visual.Controls
{
    public partial class ConnectingLine : UserControl
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From", typeof(FrameworkElement), typeof(ConnectingLine), new PropertyMetadata(default(FrameworkElement)));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To", typeof(FrameworkElement), typeof(ConnectingLine), new PropertyMetadata(default(FrameworkElement)));

        private readonly BehaviorSubject<FrameworkElement> _toChanged;
        private readonly BehaviorSubject<FrameworkElement> _fromChanged;  

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
            var froms = CenterPointsOf(FromProperty);
            var tos = CenterPointsOf(ToProperty);

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

        private IObservable<Point> CenterPointsOf(DependencyProperty property)
        {
            var desc = DependencyPropertyDescriptor.FromProperty(property, GetType());
            var changeNotifications = Observable.FromEventPattern(
                h => desc.AddValueChanged(this, h),
                h => desc.RemoveValueChanged(this, h));
            return from e in (from _ in changeNotifications
                              let f = (FrameworkElement) GetValue(property)
                              select Observable.FromEventPattern(
                                  h => f.LayoutUpdated += h,
                                  h => f.LayoutUpdated -= h)).Switch()
                   select ((FrameworkElement) GetValue(property)).CenterRelativeTo(this);
        }
    }
}
