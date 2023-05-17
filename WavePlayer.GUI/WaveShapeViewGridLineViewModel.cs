namespace WavePlayer.GUI
{
    public class WaveShapeViewGridLineViewModel
        : ViewModel
    {
        private double _horizontalOffsetPixels;
        private double _verticalLengthPixels;
        private double _thickness;

        public double HorizontalOffsetPixels
        {
            get => _horizontalOffsetPixels;

            set
            {
                if (value != _horizontalOffsetPixels)
                {
                    _horizontalOffsetPixels = value;
                    RaisePropertyChangedEvent(nameof(HorizontalOffsetPixels));
                }
            }
        }

        public double VerticalLengthPixels
        {
            get => _verticalLengthPixels;
            
            set
            {
                if (value != _verticalLengthPixels)
                {
                    _verticalLengthPixels = value;
                    RaisePropertyChangedEvent(nameof(VerticalLengthPixels));
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
