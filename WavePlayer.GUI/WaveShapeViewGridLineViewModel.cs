namespace WavePlayer.GUI
{
    public class WaveShapeViewGridLineViewModel
        : ViewModel
    {
        private double _timeSeconds;
        private double _thickness;

        public double TimeSeconds
        {
            get => _timeSeconds;
            
            set
            {
                if (value != _timeSeconds)
                {
                    _timeSeconds = value;
                    RaisePropertyChangedEvent(nameof(TimeSeconds));
                }
            }
        }

        public double Thickness
        {
            get => _thickness;
            
            set
            {
                if (value != _thickness)
                {
                    _thickness = value;
                    RaisePropertyChangedEvent(nameof(Thickness));
                }
            }
        }
    }
}
