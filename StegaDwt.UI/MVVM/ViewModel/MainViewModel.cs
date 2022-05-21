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
        public DecodeViewModel DecodeVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }

        public RelayCommand EncodeViewCommand { get; set; }
        public RelayCommand DecodeViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }

        public MainViewModel()
        {
            EncodeVM = new EncodeViewModel();
            DecodeVM = new DecodeViewModel();
            SettingsVM = new SettingsViewModel();

            CurrentView = EncodeVM;

            EncodeViewCommand = new RelayCommand(o =>
            {
                CurrentView = EncodeVM;
            });

            DecodeViewCommand = new RelayCommand(o =>
            {
                CurrentView = DecodeVM;
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVM;
            });
        }
    }
}
