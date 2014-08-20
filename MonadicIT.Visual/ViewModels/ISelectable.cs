using Caliburn.Micro;

namespace MonadicIT.Visual.ViewModels
{
    public interface ISelectable : INotifyPropertyChangedEx
    {
        string DisplayName { get; }
        bool IsSelected { get; set; }
    }
}