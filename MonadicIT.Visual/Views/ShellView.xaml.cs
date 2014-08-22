using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using MonadicIT.Visual.Backbone;

namespace MonadicIT.Visual.Views
{
    public partial class ShellView : Window, IHandle<Transmission>
    {
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
            };
        }


        public void Handle(Transmission message)
        {
            Trace.WriteLine(message);

            var animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            new DoubleAnimationUsingPath
            {
                PathGeometry = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure()
                    }
                }
            };
            new PathGeometry(new[]
            {
                new PathFigure(), 
            });
        }

        private static FrameworkElement FindNameInTemplate(Control element, string name)
        {
            return (FrameworkElement) element.Template.FindName(name, element);
        }
    }
}
