using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Experiment.WPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MarkedTime):
                        case nameof(_viewModel.PixelsPerSeconds):
                            _viewModel.WaveAreaLeftSeconds = WaveViewGrid.ActualWidth / 2 / _viewModel.PixelsPerSeconds - _viewModel.MarkedTime.TotalSeconds;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.PixelsPerSeconds):
                            _viewModel.WaveAreaMarkerSeconds = WaveViewGrid.ActualWidth / 2 / _viewModel.PixelsPerSeconds;
                            _viewModel.WaveAreaWidthSeconds = WaveViewGrid.ActualWidth / _viewModel.PixelsPerSeconds;
                            break;
                        default:
                            break;
                    }
                };

            DataContext = _viewModel;

            var points = new PointCollection();
            var lowerPoints = new Stack<Point>();
            var duration = TimeSpan.Zero;
            foreach (var sampleData in GetSampleData())
            {
                points.Add(new Point { X = sampleData.Time.TotalSeconds, Y = (1 - sampleData.MaximumValue) / 2 });
                lowerPoints.Push(new Point { X = sampleData.Time.TotalSeconds, Y = (1 - sampleData.MinimumValue) / 2 });
                if (sampleData.Time > duration)
                    duration = sampleData.Time;
            }

            _viewModel.Duration = duration;
            _viewModel.PixelsPerSeconds = 1000;
            _viewModel.MarkedTime = TimeSpan.FromSeconds(2.5);

            while (lowerPoints.Count > 0)
                points.Add(lowerPoints.Pop());

            WaveShape.Points = points;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var c = (Grid)sender;
            if (e.WidthChanged)
            {
                _viewModel.WaveAreaLeftSeconds = c.ActualWidth / 2 / _viewModel.PixelsPerSeconds - _viewModel.MarkedTime.TotalSeconds;
                _viewModel.WaveAreaMarkerSeconds = c.ActualWidth / 2 / _viewModel.PixelsPerSeconds;
                _viewModel.WaveAreaWidthSeconds = c.ActualWidth / _viewModel.PixelsPerSeconds;
            }

            if (e.HeightChanged)
            {
                _viewModel.PixelsPerSampleData = c.ActualHeight;
            }
        }
    }
}
