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
    public partial class TransmissionLayerControl : UserControl
    {
        public static readonly DependencyProperty TopTextProperty = DependencyProperty.Register(
            "TopText", typeof (string), typeof (TransmissionLayerControl), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BottomTextProperty = DependencyProperty.Register(
            "BottomText", typeof(string), typeof(TransmissionLayerControl), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked", typeof(bool), typeof(TransmissionLayerControl), new PropertyMetadata(default(bool)));

        public string TopText
        {
            get { return (string) GetValue(TopTextProperty); }
            set { SetValue(TopTextProperty, value); }
        }

        public string BottomText
        {
            get { return (string) GetValue(BottomTextProperty); }
            set { SetValue(BottomTextProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool) GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public TransmissionLayerControl()
        {
            InitializeComponent();
        }
    }
}
