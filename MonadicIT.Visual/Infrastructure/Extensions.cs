using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;

namespace MonadicIT.Visual.Infrastructure
{
    public static class Extensions
    {
        public static Point CenterRelativeTo(this FrameworkElement element, System.Windows.Media.Visual container)
        {
            return element.TransformToVisual(container).Transform(new Point(element.Width/2, element.Height/2));
        }

        public static Point TopRelativeTo(this FrameworkElement element, System.Windows.Media.Visual container)
        {
            return element.TransformToVisual(container).Transform(new Point(element.Width/2, 0));
        }
        public static Point BottomRelativeTo(this FrameworkElement element, System.Windows.Media.Visual container)
        {
            return element.TransformToVisual(container).Transform(new Point(element.Width/2, element.Height));
        }

        public static IObservable<FrameworkElement> ObserveLayoutUpdates(this FrameworkElement element)
        {
            return (from _ in Observable.FromEventPattern(
                h => element.LayoutUpdated+= h,
                h => element.LayoutUpdated-= h)
                    select element).StartWith(element);
        }

        public static IObservable<FrameworkElement> ObserveSizeChanges(this FrameworkElement element)
        {
            return (from _ in Observable.FromEventPattern<SizeChangedEventArgs>(
                h => element.SizeChanged += new SizeChangedEventHandler(h),
                h => element.SizeChanged -= new SizeChangedEventHandler(h))
                    select element).StartWith(element);
        }
    }
}