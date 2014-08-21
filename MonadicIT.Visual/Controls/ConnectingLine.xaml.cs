using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == FromProperty)
            {
                var from = CenterOf(From);
                if (double.IsNaN(from.X) || double.IsNaN(from.Y)) return;
                InnerLine.X1 = from.X;
                InnerLine.Y1 = from.Y;
            }
            else if (e.Property == ToProperty)
            {
                var to = CenterOf(To);
                if (double.IsNaN(to.X) || double.IsNaN(to.Y)) return;
                InnerLine.X2 = to.X;
                InnerLine.Y2 = to.Y;
            }
        }

        private Point CenterOf(FrameworkElement element)
        {
            return element.TransformToVisual(this).Transform(new Point(element.Width/2.0, element.Height/2.0));
        }
    }
}
