using System.Windows;

namespace MonadicIT.Visual.Infrastructure
{
    public static class Extensions
    {
        public static Point CenterRelativeTo(this FrameworkElement element, System.Windows.Media.Visual container)
        {
            return element.TransformToVisual(container).Transform(new Point(element.Width/2, element.Height/2));
        }
    }
}