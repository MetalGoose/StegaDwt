using Microsoft.Win32;
using StegaDwt.DWT;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CenterSpace.NMath.Core.Wavelet;

namespace StegaDwt.UI.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для DecodeView.xaml
    /// </summary>
    public partial class DecodeView : UserControl
    {
        private string _currentFilePath;
        private string _currentKeyPath;
        private WavFile _wavData;

        public DecodeView()
        {
            InitializeComponent();
            AddWaveletTypes();
        }

        public void DecodeMessage(object sender, RoutedEventArgs e)
        {
            ClearErrorText();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                ErrorTextBlock.Text = "Аудио файл не выбран!";
                return;
            }
            if (string.IsNullOrEmpty(_currentKeyPath))
            {
                ErrorTextBlock.Text = "Файл ключа не выбран!";
                return;
            }
            if (string.IsNullOrEmpty(DecompLvlTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите уровень декомпозиции!";
                return;
            }
            if (!int.TryParse(DecompLvlTextBox.Text, out var decompLvl))
            {
                ErrorTextBlock.Text = "Введите валидный уровень декомпозиции!";
                return;
            }

            var selectedWavelet = WaveletTypeComboBox.SelectedItem;

            if (selectedWavelet is null)
            {
                ErrorTextBlock.Text = "Выберите тип вейвлета!";
                return;
            }

            var encoder = new Encoder(_wavData, _currentKeyPath, decompLvl, (Wavelets)selectedWavelet);
            string message;

            try
            {
                message = encoder.DecodeMsg();
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Ошибка при расшифровке сообщения";
                return;
            }

            MessageTextBox.Text = message;

            ErrorTextBlock.Foreground = Brushes.Green;
            ErrorTextBlock.Text = $"Сообщение получено";
        }

        public void SelectWavFile(object sender, RoutedEventArgs e)
        {
            try
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
                _currentFilePath = filename;

                _wavData = new WavFile(filename);

                WavFileNameTextBox.Text = filename;
            }
            catch
            {
                //ignore
            }
        }

        public void SelectKeyFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();

                dialog.DefaultExt = ".txt";
                dialog.Filter = "TXT Files (*.txt)|*.txt";

                var result = dialog.ShowDialog();

                if (!(result.HasValue && result.Value)) return;

                string filename = dialog.FileName;
                _currentKeyPath = filename;
                KeyFileNameTextBox.Text = filename;
            }
            catch (Exception)
            {
                //ignored
            }
        }

        private void ClearErrorText()
        {
            ErrorTextBlock.Text = "";
            ErrorTextBlock.Foreground = Brushes.Red;
        }

        private void AddWaveletTypes()
        {
            var wavelets = Enum.GetValues(typeof(Wavelets))
                                    .Cast<Wavelets>()
                                    .ToList();

            foreach (var item in wavelets)
            {
                if (item != Wavelets.CUSTOM)
                {
                    WaveletTypeComboBox.Items.Add(item);
                }
            }
        }
    }
}
