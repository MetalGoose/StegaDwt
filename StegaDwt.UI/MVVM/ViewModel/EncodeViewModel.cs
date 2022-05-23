using Microsoft.Win32;
using StegaDwt.DWT;
using StegaDwt.UI.Core;

namespace StegaDwt.UI.MVVM.ViewModel
{
    internal class EncodeViewModel : ObservableObject
    {
        private string _currentFilePath;
        private WavFile _wavData;

        public RelayCommand ShowFileCommand { get; set; }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set
            {
                _currentFilePath = value;
                OnPropertyChanged();
            }
        }

        public EncodeViewModel()
        {
            ShowFileCommand = new RelayCommand(SelectWavFile);
        }

        public void SelectWavFile(object sender)
        {
            // Create OpenFileDialog 
            var dialog = new OpenFileDialog();

            // Set filter for file extension and default file extension
            dialog.DefaultExt = ".wav";
            dialog.Filter = "WAV Files (*.wav)|*.wav";

            // Display OpenFileDialog by calling ShowDialog method
            var result = dialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (!(result.HasValue && result.Value)) return;

            // Open document 
            string filename = dialog.FileName;
            CurrentFilePath = filename;

            _wavData = new WavFile(filename);
        }
    }
}
