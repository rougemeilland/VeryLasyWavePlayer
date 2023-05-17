using System;
using System.Collections.ObjectModel;
using WavePlayer.GUI.Properties;

namespace WavePlayer.GUI
{
    public class WaveShapeViewModel
        : ViewModel
    {
        private readonly MainWindowViewModel _rootViewModel;
        private ObservableCollection<WaveShapeViewGridLineViewModel> _gridLines;
        private double _actualWidth;
        private double _actualHeight;
        private double _musicDurationPixels;

        public WaveShapeViewModel(MainWindowViewModel rootViewModel, Action<string, bool> storyboardPlayer)
        {
            _rootViewModel = rootViewModel;

            HorizontalOffsetPixels =
                new DoubleAnimationViewModel(
                    new[]
                    {
                        "WaveShapeViewPlayAreaRectangleStoryboard",
                        "WaveShapeViewWaveShapeAreaPolygonStoryboard",
                        "WaveShapeViewTimeGridItemsStoryboard",
                    },
                    storyboardPlayer);
            PlayingTimeOffsetPixels = new DoubleAnimationViewModel("WaveShapeViewPlayingTimePositionLineStoryboard", storyboardPlayer);
        }

        public void Initialize()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ActualWidth):
                        UpdatePixelsPerSeconds();
                        UpdateHorizontalOffsetPixels();
                        UpdatePlayingTimeOffsetPixels();
                        break;
                    case nameof(PixelsPerSeconds):
                        UpdateGridLines();
                        UpdatePixelsPerSeconds();
                        UpdateMusicDurationPixels();
                        UpdateHorizontalOffsetPixels();
                        UpdatePlayingTimeOffsetPixels();
                        break;
                    default:
                        break;
                }
            };

            _rootViewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_rootViewModel.AnimationMode):
                        UpdateHorizontalOffsetPixels();
                        UpdatePlayingTimeOffsetPixels();
                        break;
                    case nameof(_rootViewModel.MarkedTime):
                        UpdateHorizontalOffsetPixels();
                        break;
                    case nameof(_rootViewModel.MusicDuration):
                        UpdateGridLines();
                        UpdatePixelsPerSeconds();
                        UpdateMusicDurationPixels();
                        UpdateHorizontalOffsetPixels();
                        UpdatePlayingTimeOffsetPixels();
                        break;
                    case nameof(_rootViewModel.PlayingTime):
                        UpdatePlayingTimeOffsetPixels();
                        break;
                    default:
                        break;
                }
            };

            void UpdateGridLines()
                => GridLines = MainWindowViewModel.GetGridLines(_rootViewModel.MusicDuration, PixelsPerSeconds, ActualHeight);

            void UpdatePixelsPerSeconds()
            {
                if (ActualWidth > 0 && _rootViewModel.MusicDuration.TotalSeconds > 0)
                {
                    var limit = ActualWidth / _rootViewModel.MusicDuration.TotalSeconds / 2;
                    if (PixelsPerSeconds < limit)
                        PixelsPerSeconds = limit;
                }
            }

            void UpdateMusicDurationPixels()
                => MusicDurationPixels = _rootViewModel.MusicDuration.TotalSeconds * PixelsPerSeconds;

            void UpdateHorizontalOffsetPixels()
            {
                HorizontalOffsetPixels.FromValue = ActualWidth / 2 - _rootViewModel.MarkedTime.TotalSeconds * PixelsPerSeconds;
                HorizontalOffsetPixels.ToValue = ActualWidth / 2 - _rootViewModel.MusicDuration.TotalSeconds * PixelsPerSeconds;
                HorizontalOffsetPixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.MarkedTime;

                if ((_rootViewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                    HorizontalOffsetPixels.StartAnimation();
                else
                    HorizontalOffsetPixels.StopAnimation();
            }

            void UpdatePlayingTimeOffsetPixels()
            {
                PlayingTimeOffsetPixels.FromValue = ActualWidth / 2 + (_rootViewModel.PlayingTime -  _rootViewModel.MarkedTime).TotalSeconds * PixelsPerSeconds;
                if ((_rootViewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                    PlayingTimeOffsetPixels.ToValue = ActualWidth / 2;
                else
                    PlayingTimeOffsetPixels.ToValue = ActualWidth / 2 + (_rootViewModel.MusicDuration - _rootViewModel.MarkedTime).TotalSeconds * PixelsPerSeconds;
                PlayingTimeOffsetPixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.PlayingTime;

                if ((_rootViewModel.AnimationMode & AnimationMode.MovePlayingPosition) != AnimationMode.None)
                    PlayingTimeOffsetPixels.StartAnimation();
                else
                    PlayingTimeOffsetPixels.StopAnimation();
            }
        }

        public double ActualWidth
        {
            get => _actualWidth;

            set
            {
                if (value != _actualWidth)
                {
                    _actualWidth = value;
                    RaisePropertyChangedEvent(nameof(ActualWidth));
                    RaisePropertyChangedEvent(nameof(HalfOfActualWidth));
                }
            }
        }

        public double ActualHeight
        {
            get => _actualHeight;

            set
            {
                if (value != _actualHeight)
                {
                    _actualHeight = value;
                    RaisePropertyChangedEvent(nameof(ActualHeight));
                    RaisePropertyChangedEvent(nameof(HalfOfActualHeight));
                }
            }
        }

        public double HalfOfActualWidth => _actualWidth / 2;
        public double HalfOfActualHeight => _actualHeight / 2;

        public double PixelsPerSeconds
        {
            get
            {
                var value = Settings.Default.WaveShapeViewPixelsPerSeconds;
                var normalizedValue = NormalizeWindowShapeViewPixelsPerSecondsValue(value);
                if (normalizedValue != value)
                {
                    Settings.Default.WaveShapeViewPixelsPerSeconds = normalizedValue;
                    Settings.Default.Save();
                }

                return normalizedValue;
            }

            set
            {
                var normalizedValue = NormalizeWindowShapeViewPixelsPerSecondsValue(value);
                if (normalizedValue != Settings.Default.WaveShapeViewPixelsPerSeconds)
                {
                    Settings.Default.WaveShapeViewPixelsPerSeconds = normalizedValue;
                    Settings.Default.Save();
                    RaisePropertyChangedEvent(nameof(PixelsPerSeconds));
                }
            }
        }

        public double MusicDurationPixels
        {
            get => _musicDurationPixels;
            
            set
            {
                if (value != _musicDurationPixels)
                {
                    _musicDurationPixels = value;
                    RaisePropertyChangedEvent(nameof(MusicDurationPixels));
                }
            }
        }

        public ObservableCollection<WaveShapeViewGridLineViewModel> GridLines
        {
            get => _gridLines;

            private set
            {
                _gridLines = value;
                RaisePropertyChangedEvent(nameof(GridLines));
            }
        }

        public DoubleAnimationViewModel HorizontalOffsetPixels { get; }
        public DoubleAnimationViewModel PlayingTimeOffsetPixels { get; }

        private static double NormalizeWindowShapeViewPixelsPerSecondsValue(double originalValue)
        {
            var normalizedValue = originalValue;
            if (normalizedValue <= 0)
                normalizedValue = 125;
            if (normalizedValue > 1000)
                normalizedValue = 1000;
            return normalizedValue;
        }
    }
}
