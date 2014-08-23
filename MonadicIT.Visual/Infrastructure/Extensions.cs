using System;
using System.Collections.Generic;
using System.Linq;
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
                h => element.LayoutUpdated += h,
                h => element.LayoutUpdated -= h)
                    select element).StartWith(element);
        }

        public static IEnumerable<IObservable<T>> ObserveElements<T>(this IObservable<IEnumerable<T>> obs)
        {
            int i = 0;
            while (true)
            {
                int idx = i++;
                yield return from enumerable in obs
                             let x = enumerable.ElementAtOrDefault(idx)
                             where !Equals(x, default(T))
                             select x;
            }
// ReSharper disable once FunctionNeverReturns
        }
    }
}