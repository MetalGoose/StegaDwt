using Microsoft.Win32;
using ScottPlot.Styles;
using StegaDwt.DWT;
using StegaDwt.UI.Theme;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CenterSpace.NMath.Core.Wavelet;

namespace StegaDwt.UI.MVVM.View
{
    /// <summary>
    /// Логика взаимодействия для EncodeView.xaml
    /// </summary>
    public partial class EncodeView : UserControl
    {
        private string _currentFilePath;
        private string _outputFilePath;
        private string _currentKeyPath;

        private WavFile _wavData;
        private readonly IStyle _plotStyle;

        public EncodeView()
        {
            InitializeComponent();
            _plotStyle = new PlotStyle();
            SetPlotsDefaultSettings();
            AddWaveletTypes();
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

        public void SelectOutputFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            var dialog = new SaveFileDialog();

            // Set filter for file extension and default file extension
            dialog.DefaultExt = ".wav";
            dialog.Filter = "WAV Files (*.wav)|*.wav";

            // Display OpenFileDialog by calling ShowDialog method
            var result = dialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (!(result.HasValue && result.Value)) return;

            // Open document 
            string filename = dialog.FileName;
            _outputFilePath = filename;

            OutputFileNameTextBox.Text = filename;
        }

        public void EncodeMessage(object sender, RoutedEventArgs e)
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
            if (string.IsNullOrEmpty(_outputFilePath))
            {
                ErrorTextBlock.Text = "Выходной аудио файл не выбран!";
                return;
            }
            if (string.IsNullOrEmpty(MessageTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите сообщение!";
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

            var wavFile = new WavFile(_currentFilePath);
            var encoder = new Encoder(wavFile, _currentKeyPath, decompLvl, (Wavelets)selectedWavelet);
            DwtTransformInfo encodingResultInfo;

            try
            {
                encodingResultInfo = encoder.EncodeMsg(MessageTextBox.Text);
            }
            catch (Exception)
            {
                ErrorTextBlock.Text = "Ошибка при кодировании сообщения";
                return;
            }

            _wavData.FloatAudioBuffer = encodingResultInfo.ResultSamples;

            EncodedWavPlot.Plot.Clear();
            DetailsWavPlot.Plot.Clear();
            ApproxWavPlot.Plot.Clear();

            EncodedWavPlot.Plot.AddSignal(
                _wavData.FloatAudioBuffer.Select(x => (double)x).ToArray(),
                _wavData.SampleRate
            );
            DetailsWavPlot.Plot.AddSignal(
                encodingResultInfo.Details.Select(x => (double)x).ToArray(),
                _wavData.SampleRate
            );
            ApproxWavPlot.Plot.AddSignal(
                encodingResultInfo.Approx.Select(x => (double)x).ToArray(),
                _wavData.SampleRate
            );

            EncodedWavPlot.Plot.AxisAuto(0);
            DetailsWavPlot.Plot.AxisAuto(0);
            ApproxWavPlot.Plot.AxisAuto(0);

            EncodedWavPlot.Render();
            DetailsWavPlot.Render();
            ApproxWavPlot.Render();

            _wavData.WriteWavFile(_outputFilePath);

            ErrorTextBlock.Foreground = Brushes.Green;
            ErrorTextBlock.Text = $"Файл {_outputFilePath} успешно создан";
        }

        private void ClearErrorText()
        {
            ErrorTextBlock.Text = "";
            ErrorTextBlock.Foreground = Brushes.Red;
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
