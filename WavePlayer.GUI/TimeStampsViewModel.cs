using System;
using System.Collections.ObjectModel;

namespace WavePlayer.GUI
{
    public class TimeStampsViewModel
        : ViewModel
    {
        private readonly MainWindowViewModel _rootViewModel;
        private ObservableCollection<TimeStampViewElementViewModel> _elements;
        private double _actualWidth;
        private double _actualHeight;

        public TimeStampsViewModel(MainWindowViewModel rootViewModel, Action<string, bool> storyboardPlayer)
        {
            _rootViewModel = rootViewModel;
            HorizontalOffsetPixels = new DoubleAnimationViewModel("TimeStampsViewItemsStoryboard", storyboardPlayer);
        }

        public void Initialize()
        {
            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ActualWidth):
                        UpdateHorizontalOffsetPixels();
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
                        break;
                    case nameof(_rootViewModel.MarkedTime):
                        UpdateHorizontalOffsetPixels();
                        break;
                    case nameof(_rootViewModel.MusicDuration):
                        UpdateElements();
                        UpdateHorizontalOffsetPixels();
                        break;
                    default:
                        break;
                }
            };

            _rootViewModel.WaveShapeView.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_rootViewModel.WaveShapeView.PixelsPerSeconds):
                        UpdateElements();
                        UpdateHorizontalOffsetPixels();
                        break;
                    default:
                        break;
                }
            };

            void UpdateElements()
                => Elements = MainWindowViewModel.GetTimeStamps(_rootViewModel.MusicDuration, _rootViewModel.WaveShapeView.PixelsPerSeconds);

            void UpdateHorizontalOffsetPixels()
            {
                HorizontalOffsetPixels.FromValue = ActualWidth / 2 - _rootViewModel.MarkedTime.TotalSeconds * _rootViewModel.WaveShapeView.PixelsPerSeconds;
                HorizontalOffsetPixels.ToValue = ActualWidth / 2 - _rootViewModel.MusicDuration.TotalSeconds * _rootViewModel.WaveShapeView.PixelsPerSeconds;
                HorizontalOffsetPixels.DurationValue = _rootViewModel.MusicDuration - _rootViewModel.MarkedTime;
                if ((_rootViewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                    HorizontalOffsetPixels.StartAnimation();
                else
                    HorizontalOffsetPixels.StopAnimation();
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

        public ObservableCollection<TimeStampViewElementViewModel> Elements
        {
            get => _elements;

            private set
            {
                _elements = value;
                RaisePropertyChangedEvent(nameof(Elements));
            }
        }

        public DoubleAnimationViewModel HorizontalOffsetPixels { get; }
    }
}
