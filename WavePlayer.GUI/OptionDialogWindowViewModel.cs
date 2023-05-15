using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;

namespace WavePlayer.GUI
{
    public class OptionDialogWindowViewModel
        : ViewModel
    {
        private IEnumerable<ComboBoxSelectionItem<string>> _cultureSelectionItems;

        public IEnumerable<ComboBoxSelectionItem<string>> CultureSelectionItems
        {
            get => _cultureSelectionItems;

            set
            {
                _cultureSelectionItems = value;
                RaisePropertyChangedEvent(nameof(CultureSelectionItems));
            }
        }

        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
    }
}
