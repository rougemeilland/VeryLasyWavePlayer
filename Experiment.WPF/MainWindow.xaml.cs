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
            var storyboard1 = (Storyboard)Resources["MoveRectangle1"];
            var storyboard2 = (Storyboard)Resources["MoveRectangle2"];
            _viewModel.From1 = 100;
            _viewModel.To1 = -500;
            _viewModel.DurationTime1 = TimeSpan.FromSeconds(3);
            _viewModel.From2 = 100;
            _viewModel.To2 = 700;
            _viewModel.DurationTime2 = TimeSpan.FromSeconds(3);
#if true
            storyboard1.Begin();
            storyboard2.Begin();
            _viewModel.IsRunning = true;
#else
            if (_viewModel.IsRunning)
            {
                storyboard1.Stop();
                storyboard2.Stop();
                _viewModel.IsRunning = false;
            }
            else
            {
                storyboard1.Begin();
                storyboard2.Begin();
                _viewModel.IsRunning = true;
            }
#endif
        }
    }
}
