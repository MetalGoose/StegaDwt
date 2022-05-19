using StegaDwt.UI.Core;

namespace StegaDwt.UI.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public EncodeViewModel EncodeVM { get; set; }

        public MainViewModel()
        {
            EncodeVM = new EncodeViewModel();
            CurrentView = EncodeVM;
        }
    }
}
