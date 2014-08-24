using Caliburn.Micro;

namespace MonadicIT.Visual.ViewModels
{
    public abstract class SelectableViewModelBase : PropertyChangedBase, ISelectable
    {
        private bool _isSelected;
        public abstract string DisplayName { get; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange("IsSelected");
            }
        }
    }
}