using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.WPF
{
    public class MainViewModel
        : ViewModel
    {
        private TimeSpan _duration;
        private TimeSpan _markedTime;
        private double _pixelsPerSeconds;
        private double _pixelsPerSampleData;
        private double _waveAreaLeftSeconds;
        private double _waveAreaWidthSeconds;
        private double _waveAreaCenterSeconds;

        public TimeSpan Duration
        {
            get => _duration;

            set
            {
                if (value != _duration)
                {
                    _duration = value;
                    RaisePropertyChangedEvent(nameof(Duration));
                    RaisePropertyChangedEvent(nameof(DurationSeconds));
                }
            }
        }

        public double DurationSeconds => _duration.TotalSeconds;

        public TimeSpan MarkedTime
        {
            get => _markedTime;

            set
            {
                if (value != _markedTime)
                {
                    _markedTime = value;
                    RaisePropertyChangedEvent(nameof(MarkedTime));
                }
            }
        }

        public double WaveAreaLeftSeconds
        {
            get => _waveAreaLeftSeconds;

            set
            {
                if (value != _waveAreaLeftSeconds)
                {
                    _waveAreaLeftSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveAreaLeftSeconds));
                }
            }
        }

        public double WaveAreaWidthSeconds
        {
            get => _waveAreaWidthSeconds;
            
            set
            {
                if (_waveAreaWidthSeconds != value)
                {
                    _waveAreaWidthSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveAreaWidthSeconds));
                }
            }
        }
        public double WaveAreaMarkerSeconds
        {
            get => _waveAreaCenterSeconds;

            set
            {
                if (_waveAreaCenterSeconds != value)
                {
                    _waveAreaCenterSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveAreaMarkerSeconds));
                }
            }
        }

        public double PixelsPerSeconds
        {
            get => _pixelsPerSeconds;

            set
            {
                if (value != _pixelsPerSeconds)
                {
                    _pixelsPerSeconds = value;
                    RaisePropertyChangedEvent(nameof(PixelsPerSeconds));
                    RaisePropertyChangedEvent(nameof(VerticalLineTickness));
                }
            }
        }

        public double PixelsPerSampleData
        {
            get => _pixelsPerSampleData;

            set
            {
                if (value != _pixelsPerSampleData)
                {
                    _pixelsPerSampleData = value;
                    RaisePropertyChangedEvent(nameof(PixelsPerSampleData));
                    RaisePropertyChangedEvent(nameof(HorizontalLineTickness));
                }
            }
        }

        public double HorizontalLineTickness => 1.0 / _pixelsPerSampleData ;
        public double VerticalLineTickness => 1.0 / _pixelsPerSeconds;
    }
}
