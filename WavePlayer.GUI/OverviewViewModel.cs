using System;

namespace WavePlayer.GUI
{
    public class OverviewViewModel
        : ViewModel
    {
        private readonly MainWindowViewModel _rootViewModel;
        private double _horizontalMagnification;
        private double _markedRangeWidthPixels;
        private double _actualWidth;
        private double _actualHeight;

        public OverviewViewModel(MainWindowViewModel rootViewModel, Action<string, bool> storyboardPlayer)
        {
            _rootViewModel = rootViewModel;
            MarkedRangeLeftPixels = new DoubleAnimationViewModel("OverviewViewPlayAreaRectangleStoryboard", storyboardPlayer);
            PlayingTimePixels = new DoubleAnimationViewModel("OverviewViewPlayingTimePositionLineStoryboard", storyboardPlayer);
            MarkedTimePixels = new DoubleAnimationViewModel("OverviewViewMarkedTimePositionLineStoryboard", storyboardPlayer);
        }

        public void Initialize()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ActualWidth):
                        UpdatePlayingTimePixels();
                        UpdateMarkedTimePixels();
                        UpdateMarkedRangeWidthPixels();
                        UpdateMarkedRangeLeftPixels();
                        UpdateHorizontalMagnification();
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
                        UpdateMarkedRangeLeftPixels();
                        UpdateMarkedTimePixels();
                        UpdatePlayingTimePixels();
                        break;
                    case nameof(_rootViewModel.MarkedTime):
                        UpdateMarkedTimePixels();
                        UpdateMarkedRangeLeftPixels();
                        break;
                    case nameof(_rootViewModel.MusicDuration):
                        UpdatePlayingTimePixels();
                        UpdateMarkedTimePixels();
                        UpdateMarkedRangeWidthPixels();
                        UpdateMarkedRangeLeftPixels();
                        UpdateHorizontalMagnification();
                        break;
                    case nameof(_rootViewModel.PlayingTime):
                        UpdatePlayingTimePixels();
                        break;
                    default:
                        break;
                }
            };

            _rootViewModel.WaveShapeView.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_rootViewModel.WaveShapeView.ActualWidth):
                        UpdateMarkedRangeWidthPixels();
                        UpdateMarkedRangeLeftPixels();
                        break;
                    case nameof(_rootViewModel.WaveShapeView.PixelsPerSeconds):
                        UpdateMarkedRangeWidthPixels();
                        UpdateMarkedRangeLeftPixels();
                        break;
                    default:
                        break;
                }
            };

            void UpdateHorizontalMagnification()
                => HorizontalMagnification = ActualWidth / _rootViewModel.MusicDuration.TotalSeconds;

            void UpdateMarkedRangeWidthPixels()
            {
                if (_rootViewModel.WaveShapeView.PixelsPerSeconds > 0 && _rootViewModel.MusicDuration.TotalSeconds > 0)
                {
                    MarkedRangeWidthPixels =
                        _rootViewModel.WaveShapeView.ActualWidth
                        / _rootViewModel.WaveShapeView.PixelsPerSeconds
                        / _rootViewModel.MusicDuration.TotalSeconds
                        * ActualWidth;
                }
            }

            void UpdateMarkedRangeLeftPixels()
            {
                if (_rootViewModel.WaveShapeView.PixelsPerSeconds > 0 && _rootViewModel.MusicDuration > TimeSpan.Zero)
                {
                    MarkedRangeLeftPixels.FromValue =
                        (_rootViewModel.MarkedTime.TotalSeconds - _rootViewModel.WaveShapeView.ActualWidth / 2 / _rootViewModel.WaveShapeView.PixelsPerSeconds)
                        / _rootViewModel.MusicDuration.TotalSeconds
                        * ActualWidth;
                }

                if (_rootViewModel.WaveShapeView.PixelsPerSeconds > 0 && _rootViewModel.MusicDuration > TimeSpan.Zero)
                {
                    MarkedRangeLeftPixels.ToValue =
                        ActualWidth - ActualWidth * _rootViewModel.WaveShapeView.ActualWidth / 2 / _rootViewModel.WaveShapeView.PixelsPerSeconds / _rootViewModel.MusicDuration.TotalSeconds;
                }

                MarkedRangeLeftPixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.MarkedTime;

                if ((_rootViewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                    MarkedRangeLeftPixels.StartAnimation();
                else
                    MarkedRangeLeftPixels.StopAnimation();
            }

            void UpdateMarkedTimePixels()
            {
                if (_rootViewModel.MusicDuration > TimeSpan.Zero)
                    MarkedTimePixels.FromValue = _rootViewModel.MarkedTime.TotalSeconds / _rootViewModel.MusicDuration.TotalSeconds * ActualWidth;
                MarkedTimePixels.ToValue = ActualWidth;
                MarkedTimePixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.MarkedTime;

                if ((_rootViewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                    MarkedTimePixels.StartAnimation();
                else
                    MarkedTimePixels.StopAnimation();
            }

            void UpdatePlayingTimePixels()
            {
                if (_rootViewModel.MusicDuration > TimeSpan.Zero)
                    PlayingTimePixels.FromValue = _rootViewModel.PlayingTime.TotalSeconds / _rootViewModel.MusicDuration.TotalSeconds * ActualWidth;
                PlayingTimePixels.ToValue = ActualWidth;
                PlayingTimePixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.PlayingTime;

                if ((_rootViewModel.AnimationMode & AnimationMode.MovePlayingPosition) != AnimationMode.None)
                    PlayingTimePixels.StartAnimation();
                else
                    PlayingTimePixels.StopAnimation();
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
                }
            }
        }

        public double HorizontalMagnification
        {
            get => _horizontalMagnification;

            private set
            {
                if (value != _horizontalMagnification)
                {
                    _horizontalMagnification = value;
                    RaisePropertyChangedEvent(nameof(HorizontalMagnification));
                }
            }
        }

        public double MarkedRangeWidthPixels
        {
            get => _markedRangeWidthPixels;

            private set
            {
                if (value != _markedRangeWidthPixels)
                {
                    _markedRangeWidthPixels = value;
                    RaisePropertyChangedEvent(nameof(MarkedRangeWidthPixels));
                }
            }
        }

        public DoubleAnimationViewModel MarkedRangeLeftPixels { get; }

        public DoubleAnimationViewModel MarkedTimePixels { get; }

        public DoubleAnimationViewModel PlayingTimePixels { get; }
    }
}
