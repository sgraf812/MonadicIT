using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonadicIT.Visual.Controls
{
    public sealed class TransmissionNode : DependencyObject
    {
        public static readonly DependencyProperty TopTextProperty = DependencyProperty.RegisterAttached(
            "TopText", typeof(string), typeof(TransmissionNode), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BottomTextProperty = DependencyProperty.RegisterAttached(
            "BottomText", typeof(string), typeof(TransmissionNode), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width", typeof(int), typeof(TransmissionNode), new PropertyMetadata(default(int)));

        public static void SetTopText(DependencyObject element, string value)
        {
            element.SetValue(TopTextProperty, value);
        }

        public static string GetTopText(DependencyObject element)
        {
            return (string) element.GetValue(TopTextProperty);
        }

        public static void SetBottomText(DependencyObject element, string value)
        {
            element.SetValue(BottomTextProperty, value);
        }

        public static string GetBottomText(DependencyObject element)
        {
            return (string) element.GetValue(BottomTextProperty);
        }

        public static void SetWidth(DependencyObject element, int value)
        {
            element.SetValue(WidthProperty, value);
        }

        public static int GetWidth(DependencyObject element)
        {
            return (int) element.GetValue(WidthProperty);
        }
    }
}
