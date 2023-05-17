using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WavePlayer.GUI
{
    public class DoubleAnimationViewModel
        : ViewModel
    {
        private readonly IEnumerable< string> _storyboardNames;
        private readonly Action<string, bool> _storyboardPlayer;
        private double _fromValue;
        private double _toValue;
        private TimeSpan _durationValue;

        public DoubleAnimationViewModel(string storyboardName, Action<string, bool> storyboardPlayer)
            : this(new[] { storyboardName }, storyboardPlayer)
        {
        }

        public DoubleAnimationViewModel(IEnumerable<string> storyboardNames, Action<string, bool> storyboardPlayer)
        {
            _storyboardNames = storyboardNames.ToList();
            _storyboardPlayer = storyboardPlayer;
        }

        public double From { get; private set; }
        public double To { get; private set; }
        public Duration Duration { get; private set; }

        public void StartAnimation()
        {
            From = _fromValue;
            To = _toValue;
            Duration = _durationValue;
            RaisePropertyChangedEvent(nameof(From));
            RaisePropertyChangedEvent(nameof(To));
            RaisePropertyChangedEvent(nameof(Duration));
            if (!double.IsNaN(_fromValue) &&
                !double.IsInfinity(_fromValue) &&
                !double.IsNaN(_toValue) &&
                !double.IsInfinity(_toValue) &&
                _durationValue >= TimeSpan.Zero)
            {
                foreach (var storyboardName in _storyboardNames)
                    _storyboardPlayer(storyboardName, false);
                foreach (var storyboardName in _storyboardNames)
                    _storyboardPlayer(storyboardName, true);
            }
        }

        public void StopAnimation()
        {
            From = _fromValue;
            To = _fromValue;
            Duration = TimeSpan.Zero;
            RaisePropertyChangedEvent(nameof(From));
            RaisePropertyChangedEvent(nameof(To));
            RaisePropertyChangedEvent(nameof(Duration));
            if (!double.IsNaN(_fromValue) &&
                !double.IsInfinity(_fromValue) &&
                !double.IsNaN(_toValue) &&
                !double.IsInfinity(_toValue) &&
                _durationValue >= TimeSpan.Zero)
            {
                foreach (var storyboardName in _storyboardNames)
                    _storyboardPlayer(storyboardName, false);
                foreach (var storyboardName in _storyboardNames)
                    _storyboardPlayer(storyboardName, true);
            }
        }

        internal double FromValue
        {
            get => _fromValue;

            set
            {
                if (value != _fromValue)
                {
                    _fromValue = value;
                    RaisePropertyChangedEvent(nameof(FromValue));
                }
            }
        }

        internal double ToValue
        {
            get => _toValue;

            set
            {
                if (value != _toValue)
                {
                    _toValue = value;
                    RaisePropertyChangedEvent(nameof(ToValue));
                }
            }
        }

        internal TimeSpan DurationValue
        {
            get => _durationValue;

            set
            {
                var normalizedValue = value;
                if (normalizedValue < TimeSpan.Zero)
                    normalizedValue = TimeSpan.Zero;
                if (normalizedValue != _durationValue)
                {
                    _durationValue = normalizedValue;
                    RaisePropertyChangedEvent(nameof(DurationValue));
                }
            }
        }
    }
}
