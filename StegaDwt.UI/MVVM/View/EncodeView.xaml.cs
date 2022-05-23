using Microsoft.Win32;
using ScottPlot.Styles;
using StegaDwt.UI.Theme;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StegaDwt.UI.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для EncodeView.xaml
    /// </summary>
    public partial class EncodeView : UserControl
    {
        private string _currentFilePath;
        private string _currentKeyPath;
        private WavFile _wavData;
        private IStyle _plotStyle;

        public EncodeView()
        {
            InitializeComponent();
            _plotStyle = new PlotStyle();
            SetPlotsDefaultSettings();
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

                OriginWavPlot.Plot.Clear();

                OriginWavPlot.Plot.AddSignal(
                    _wavData.FloatAudioBuffer.Select(x => (double)x).ToArray(),
                    _wavData.SampleRate
                );

                OriginWavPlot.Plot.AxisAuto(0);
                OriginWavPlot.Render();
                OriginWavPlot.Refresh();
                InvalidateVisual();
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

        private void SetPlotsDefaultSettings()
        {
            OriginWavPlot.Plot.Title("Исходный файл");
            OriginWavPlot.Plot.Style(_plotStyle);
            OriginWavPlot.Plot.XLabel("Time (seconds)");
            OriginWavPlot.Plot.YLabel("Audio Value");

            EncodedWavPlot.Plot.Title("Файл с сообщением");
            EncodedWavPlot.Plot.Style(_plotStyle);
            EncodedWavPlot.Plot.XLabel("Time (seconds)");
            EncodedWavPlot.Plot.YLabel("Audio Value");

            DetailsWavPlot.Plot.Title("Детализация");
            DetailsWavPlot.Plot.Style(_plotStyle);
            DetailsWavPlot.Plot.XLabel("Time (seconds)");
            DetailsWavPlot.Plot.YLabel("Audio Value");

            ApproxWavPlot.Plot.Title("Аппроксимация");
            ApproxWavPlot.Plot.Style(_plotStyle);
            ApproxWavPlot.Plot.XLabel("Time (seconds)");
            ApproxWavPlot.Plot.YLabel("Audio Value");
        }

        private void EncodeMessage(object sender, RoutedEventArgs e)
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
            if (string.IsNullOrEmpty(MessageTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите сообщение!";
                return;
            }
        }

        private void ClearErrorText()
        {
            ErrorTextBlock.Text = "";
        }
    }
}
