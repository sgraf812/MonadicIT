using Caliburn.Micro;

namespace MonadicIT.Visual.Views
{
    public class ClosableViewModel : PropertyChangedBase, IClose
    {
        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value.Equals(_isOpen)) return;
                _isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public void TryClose(bool? dialogResult = null)
        {
            if (dialogResult == true)
            {
                IsOpen = false;
            }
        }
    }
}