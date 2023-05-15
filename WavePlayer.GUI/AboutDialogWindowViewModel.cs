using System.Windows.Input;

namespace WavePlayer.GUI
{
    public class AboutDialogWindowViewModel
        : ViewModel
    {
        public string Version { get; set; }
        public string Copyright { get; set; }
        public string Url { get; set; }
        public ICommand OkCommand { get; set; }
    }
}
