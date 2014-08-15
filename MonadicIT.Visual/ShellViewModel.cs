using System;
using System.Windows;
using Caliburn.Micro;
using Caliburn.Micro.ReactiveUI;

namespace MonadicIT.Visual
{
    public class ShellViewModel : ReactivePropertyChangedBase
    {
        public SourceSinkViewModel SourceSink { get; set; }
        public BindableCollection<Model> Items { get; private set; }

        public ShellViewModel()
        {
            Items = new BindableCollection<Model>{
            new Model { Id = Guid.NewGuid() },
            new Model { Id = Guid.NewGuid() },
            new Model { Id = Guid.NewGuid() },
            new Model { Id = Guid.NewGuid() }
        };
            SourceSink = new SourceSinkViewModel();
        }

        public void Add()
        {
            Items.Add(new Model { Id = Guid.NewGuid() });
        }

        public void Remove(Model child)
        {
            Items.Remove(child);
        }
    }

    public class Model
    {
        public Guid Id { get; set; }
    }

    public class SourceSinkViewModel : ReactiveScreen
    {
        public void Greet()
        {
            MessageBox.Show("hi");
        }
    }
}