using System.Windows;
using System.Linq;

namespace WavePlayer.GUI
{
    /// <summary>
    /// OptionDialogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionDialogWindow : Window
    {
        public OptionDialogWindow() => InitializeComponent();

        public string SelectedCulture
        {
            get
            {
                var viewModel = (OptionDialogWindowViewModel)DataContext;
                var selectedItem =
                    viewModel.CultureSelectionItems
                    .Skip(CultureSelectionComboBox.SelectedIndex)
                    .FirstOrDefault();
                return
                    selectedItem is null
                    ? ""
                    : selectedItem.Value;
            }

            set
            {
                var viewModel = (OptionDialogWindowViewModel)DataContext;
                var selectedItem = 
                    viewModel.CultureSelectionItems
                    .Select((item, index) => new { index, value = item.Value })
                    .Where(item => item.value == value)
                    .FirstOrDefault();
                CultureSelectionComboBox.SelectedIndex =
                    selectedItem is null
                    ? 0
                    : selectedItem.index;
            }
        }
    }
}
