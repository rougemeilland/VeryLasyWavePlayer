namespace WavePlayer.GUI
{
    public class TimeStampViewElementViewModel
        : ViewModel
    {
        private double _horizontalPositionPixels;
        private string _timeText;

        public double HorizontalPositionPixels
        {
            get => _horizontalPositionPixels;

            set
            {
                if (value != _horizontalPositionPixels)
                {
                    _horizontalPositionPixels = value;
                    RaisePropertyChangedEvent(nameof(HorizontalPositionPixels));
                }
            }
        }

        public string TimeText
        {
            get => _timeText;

            set
            {
                if (value != _timeText)
                {
                    _timeText = value;
                    RaisePropertyChangedEvent(nameof(TimeText));
                }
            }
        }
    }
}
