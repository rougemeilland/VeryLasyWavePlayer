using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Experiment.WPF
{
    public class MainWindowViewModel
        : ViewModel
    {
        private bool _isRunning;
        private double _from1;
        private double _to1;
        private TimeSpan _durationTime1;
        private double _from2;
        private double _to2;
        private TimeSpan _durationTime2;

        public bool IsRunning
        {
            get => _isRunning;

            set
            {
                if (value != _isRunning)
                {
                    _isRunning = value;
                    RaisePropertyChangedEvent(nameof(IsRunning));
                }
            }
        }

        public double From1
        {
            get => _from1;

            set
            {
                if (value != _from1)
                {
                    _from1 = value;
                    RaisePropertyChangedEvent(nameof(From1));
                }
            }
        }

        public double To1
        {
            get => _to1;

            set
            {
                if (value != _to1)
                {
                    _to1 = value;
                    RaisePropertyChangedEvent(nameof(To1));
                }
            }
        }

        public TimeSpan Duration1 => _durationTime1;

        internal TimeSpan DurationTime1
        {
            get => _durationTime1;
            
            set
            {
                if (value != _durationTime1)
                {
                    _durationTime1 = value;
                    RaisePropertyChangedEvent(nameof(Duration1));
                    RaisePropertyChangedEvent(nameof(DurationTime1));
                }
            }
        }

        public double From2
        {
            get => _from2;

            set
            {
                if (value != _from2)
                {
                    _from2 = value;
                    RaisePropertyChangedEvent(nameof(From2));
                }
            }
        }

        public double To2
        {
            get => _to2;

            set
            {
                if (value != _to2)
                {
                    _to2 = value;
                    RaisePropertyChangedEvent(nameof(To2));
                }
            }
        }

        public TimeSpan Duration2 => _durationTime2;

        internal TimeSpan DurationTime2
        {
            get => _durationTime2;

            set
            {
                if (value != _durationTime2)
                {
                    _durationTime2 = value;
                    RaisePropertyChangedEvent(nameof(Duration2));
                    RaisePropertyChangedEvent(nameof(DurationTime2));
                }
            }
        }
    }
}
