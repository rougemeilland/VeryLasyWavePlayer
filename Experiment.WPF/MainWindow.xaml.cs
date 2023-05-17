using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        default:
                            break;
                    }
                };

            DataContext = _viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = Clipboard.GetDataObject();
            if (!(data is null))
            {
                var text = data.GetData(DataFormats.Text, true) as string;
                if (!string.IsNullOrEmpty(text))
                {
                    var pattern = new Regex(@"^\s*\[?((?<m>\d+):)?(?<s>\d+(\.\d+))?\]?\s*$", RegexOptions.Compiled);
                    var match = pattern.Match(text);
                    if (match.Success)
                    {
                        var m = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat) : 0;
                        var s = double.Parse(match.Groups["s"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                        var time = TimeSpan.FromSeconds(m * 60 + s);
                        PastedText.Text = $"{Math.Floor(time.TotalMinutes):F0}:{time.Seconds:D2}.{time.Milliseconds:D3}";
                    }
                }
            }
        }
    }
}
