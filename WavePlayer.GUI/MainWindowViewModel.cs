using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using WavePlayer.GUI.Properties;

namespace WavePlayer.GUI
{
    public class MainWindowViewModel
        : ViewModel
    {
        private string _currentMusicFilePath;
        private MusicPlayingStatusType _musicPlayingStatus;
        private string _musicPlayingStatusText;
        private TimeSpan _playingTime;
        private TimeSpan _markedTime;
        private TimeSpan _musicDuration;
        private bool _copiedMarkedTime;
        private PointCollection _waveShapePoints;
        private double _waveShapeViewPixelsPerSampleData;
        private double _waveShapeViewHorizontalOffsetSeconds;
        private double _waveShapeViewWidthSeconds;
        private ObservableCollection<WaveShapeViewGridLineViewModel> _waveShapeViewGridLines;
        private double _waveShapeViewHorizontalOffsetPixels;
        private ObservableCollection<TimeStampViewElementViewModel> _timeStampsViewElements;
        private double _overViewWidthPixels;
        private double _overViewMagnification;
        private double _overViewMarkedRangeLeftPixels;
        private double _overViewMarkedRangeWidthPixels;
        private double _waveShapeViewActualWidth;
        private double _waveShapeViewActualHeight;
        private double _overViewActualWidth;
        private double _overViewActualHeight;
        private double _timeStampsViewActualWidth;
        private double _timeStampsViewActualHeight;
        private double _overViewMarkedTimePixels;
        private double _overViewPlayingTimePixels;

        public Command OpenCommand { get; set; }
        public Command ExitCommand { get; set; }
        public Command OptionCommand { get; set; }
        public Command AboutCommand { get; set; }
        public Command Play10msecAndPauseCommand { get; set; }
        public Command PlayAndMoveMarkerOrPauseCommand { get; set; }
        public Command PlayFromMarkerCommand { get; set; }
        public Command PauseCommand { get; set; }
        public Command PositionForward100msecCommand { get; set; }
        public Command PositionBackward100msecCommand { get; set; }
        public Command PositionForward1secCommand { get; set; }
        public Command PositionBackward1secCommand { get; set; }
        public Command PositionForward10secCommand { get; set; }
        public Command PositionBackward10secCommand { get; set; }
        public Command PositionForward60secCommand { get; set; }
        public Command PositionBackward60secCommand { get; set; }
        public Command PositionHomeCommand { get; set; }
        public Command VolumeUpCommand { get; set; }
        public Command VolumeDownCommand { get; set; }
        public Command CopyMarkerTextCommand { get; set; }
        public Command ExpandTimeLineCommand { get; set; }
        public Command ShrinkTimeLineCommand { get; set; }

        public string CurrentCultureName
        {
            get => Settings.Default.Culture;

            set
            {
                if (value != Settings.Default.Culture)
                {
                    Settings.Default.Culture = value;
                    Settings.Default.Save();
                    RaisePropertyChangedEvent(nameof(CurrentCultureName));
                }
            }
        }

        public string CurrentMusicFilePath
        {
            get => _currentMusicFilePath;

            set
            {
                if (value != _currentMusicFilePath)
                {
                    _currentMusicFilePath = value;
                    RaisePropertyChangedEvent(nameof(IsLoadedMusicFile));
                    RaisePropertyChangedEvent(nameof(CurrentMusicFilePath));
                }
            }
        }

        public bool IsLoadedMusicFile => !string.IsNullOrEmpty(_currentMusicFilePath);

        public string LatestOpenedMusicFilePath
        {
            get => Settings.Default.LatestOpenedFilePath;

            set
            {
                if (value != Settings.Default.LatestOpenedFilePath)
                {
                    Settings.Default.LatestOpenedFilePath = value;
                    Settings.Default.Save();
                    RaisePropertyChangedEvent(nameof(LatestOpenedMusicFilePath));
                }
            }
        }

        public TimeSpan MusicDuration
        {
            get => _musicDuration;

            set
            {
                if (value != _musicDuration)
                {
                    _musicDuration = value;
                    RaisePropertyChangedEvent(nameof(MusicDuration));
                    RaisePropertyChangedEvent(nameof(MusicDurationSeconds));
                    RaisePropertyChangedEvent(nameof(MusicDurationText));
                }
            }
        }

        public double MusicDurationSeconds => _musicDuration.TotalSeconds;

        public string MusicDurationText => FormatTime(_musicDuration);

        public MusicPlayingStatusType MusicPlayingStatus
        {
            get => _musicPlayingStatus;

            set
            {
                if (value != _musicPlayingStatus)
                {
                    _musicPlayingStatus = value;

                    switch (value)
                    {
                        case MusicPlayingStatusType.None:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_NONE;
                            break;
                        case MusicPlayingStatusType.Loading:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_LOADING;
                            break;
                        case MusicPlayingStatusType.Analyzing:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_ANALYZING;
                            break;
                        case MusicPlayingStatusType.Ready:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_READY;
                            break;
                        case MusicPlayingStatusType.Playing:
                        case MusicPlayingStatusType.PlayingWithMarkerMovement:
                        case MusicPlayingStatusType.Stepping:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_PLAYING;
                            break;
                        case MusicPlayingStatusType.Paused:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_PAUSED;
                            break;
                        case MusicPlayingStatusType.Error:
                            MusicPlayingStatusText = Resources.STATUS_TEXT_ERROR;
                            break;
                        default:
                            break;
                    }

                    RaisePropertyChangedEvent(nameof(MusicPlayingStatus));
                    RaisePropertyChangedEvent(nameof(IsValidMusicPlayingStatus));
                    RaisePropertyChangedEvent(nameof(IsVisiblePlayView));
                }
            }
        }

        public bool IsValidMusicPlayingStatus => _musicPlayingStatus != MusicPlayingStatusType.None;

        public string MusicPlayingStatusText
        {
            get => _musicPlayingStatusText;

            private set
            {
                if (value != _musicPlayingStatusText)
                {
                    _musicPlayingStatusText = value;
                    RaisePropertyChangedEvent(nameof(MusicPlayingStatusText));
                }
            }
        }

        public TimeSpan MarkedTime
        {
            get => _markedTime;

            set
            {
                var normalizedTime = NormalizeTime(value);
                if (normalizedTime != _markedTime)
                {
                    _markedTime = normalizedTime;
                    RaisePropertyChangedEvent(nameof(MarkedTime));
                    RaisePropertyChangedEvent(nameof(MarkedTimeSeconds));
                    RaisePropertyChangedEvent(nameof(MarkedTimeText));
                }
            }
        }

        public double MarkedTimeSeconds => _markedTime.TotalSeconds;

        public string MarkedTimeText => FormatTime(_markedTime);

        public TimeSpan PlayingTime
        {
            get => _playingTime;

            set
            {
                var normalizedTime = NormalizeTime(value);
                if (normalizedTime != _playingTime)
                {
                    _playingTime = normalizedTime;
                    RaisePropertyChangedEvent(nameof(PlayingTime));
                    RaisePropertyChangedEvent(nameof(PlayingTimeSeconds));
                    RaisePropertyChangedEvent(nameof(PlayingTimeText));
                }
            }
        }

        public double PlayingTimeSeconds => _playingTime.TotalSeconds;

        public string PlayingTimeText => FormatTime(_playingTime);

        public bool IsVisiblePlayView
        {
            get
            {
                switch (_musicPlayingStatus)
                {
                    case MusicPlayingStatusType.Ready:
                    case MusicPlayingStatusType.Playing:
                    case MusicPlayingStatusType.PlayingWithMarkerMovement:
                    case MusicPlayingStatusType.Paused:
                    case MusicPlayingStatusType.Stepping:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public int PlayerVolume
        {
            get => Settings.Default.PlayerVolume;

            set
            {
                if (value != Settings.Default.PlayerVolume)
                {
                    Settings.Default.PlayerVolume = value;
                    Settings.Default.Save();
                    RaisePropertyChangedEvent(nameof(PlayerVolume));
                    RaisePropertyChangedEvent(nameof(VolumeLevel1));
                    RaisePropertyChangedEvent(nameof(VolumeLevel2));
                    RaisePropertyChangedEvent(nameof(VolumeLevel3));
                    RaisePropertyChangedEvent(nameof(VolumeLevel4));
                    RaisePropertyChangedEvent(nameof(VolumeLevel5));
                    RaisePropertyChangedEvent(nameof(VolumeLevel6));
                    RaisePropertyChangedEvent(nameof(VolumeLevel7));
                    RaisePropertyChangedEvent(nameof(VolumeLevel8));
                    RaisePropertyChangedEvent(nameof(VolumeLevel9));
                    RaisePropertyChangedEvent(nameof(VolumeLevel10));
                }
            }
        }

        public bool VolumeLevel1 => PlayerVolume > 0;
        public bool VolumeLevel2 => PlayerVolume > 10;
        public bool VolumeLevel3 => PlayerVolume > 20;
        public bool VolumeLevel4 => PlayerVolume > 30;
        public bool VolumeLevel5 => PlayerVolume > 40;
        public bool VolumeLevel6 => PlayerVolume > 50;
        public bool VolumeLevel7 => PlayerVolume > 60;
        public bool VolumeLevel8 => PlayerVolume > 70;
        public bool VolumeLevel9 => PlayerVolume > 80;
        public bool VolumeLevel10 => PlayerVolume > 90;

        public bool CopiedMarkedTime
        {
            get => _copiedMarkedTime;

            set
            {
                if (value != _copiedMarkedTime)
                {
                    _copiedMarkedTime = value;
                    RaisePropertyChangedEvent(nameof(CopiedMarkedTime));
                }
            }
        }

        public double OverViewActualWidth
        {
            get => _overViewActualWidth;

            set
            {
                if (value != _overViewActualWidth)
                {
                    _overViewActualWidth = value;
                    RaisePropertyChangedEvent(nameof(OverViewActualWidth));
                }
            }
        }

        public double OverViewActualHeight
        {
            get => _overViewActualHeight;

            set
            {
                if (value != _overViewActualHeight)
                {

                    _overViewActualHeight = value;
                    RaisePropertyChangedEvent(nameof(OverViewActualHeight));
                }
            }
        }

        public double OverViewMagnification
        {
            get => _overViewMagnification;

            set
            {
                if (value != _overViewMagnification)
                {
                    _overViewMagnification = value;
                    RaisePropertyChangedEvent(nameof(OverViewMagnification));
                }
            }
        }

        public double OverViewWidthPixels
        {
            get => _overViewWidthPixels;

            set
            {
                if (value != _overViewWidthPixels)
                {
                    _overViewWidthPixels = value;
                    RaisePropertyChangedEvent(nameof(OverViewWidthPixels));
                }
            }
        }

        public double OverViewMarkedRangeLeftPixels
        {
            get => _overViewMarkedRangeLeftPixels;

            set
            {
                if (value != _overViewMarkedRangeLeftPixels)
                {
                    _overViewMarkedRangeLeftPixels = value;
                    RaisePropertyChangedEvent(nameof(OverViewMarkedRangeLeftPixels));
                }
            }
        }

        public double OverViewMarkedRangeWidthPixels
        {
            get => _overViewMarkedRangeWidthPixels;

            set
            {
                if (value != _overViewMarkedRangeWidthPixels)
                {
                    _overViewMarkedRangeWidthPixels = value;
                    RaisePropertyChangedEvent(nameof(OverViewMarkedRangeWidthPixels));
                }
            }
        }

        public double OverViewMarkedTimePixels
        {
            get => _overViewMarkedTimePixels;
            
            set
            {
                if (value != OverViewMarkedTimePixels)
                {
                    _overViewMarkedTimePixels = value;
                    RaisePropertyChangedEvent(nameof(OverViewMarkedTimePixels));
                }
            }
        }

        public double OverViewPlayingTimePixels
        {
            get => _overViewPlayingTimePixels;
            
            set
            {
                if (value != _overViewPlayingTimePixels)
                {
                    _overViewPlayingTimePixels = value;
                    RaisePropertyChangedEvent(nameof(OverViewPlayingTimePixels));
                }
            }
        }

        public double TimeStampsViewActualWidth
        {
            get => _timeStampsViewActualWidth;

            set
            {
                if (value != _timeStampsViewActualWidth)
                {
                    _timeStampsViewActualWidth = value;
                    RaisePropertyChangedEvent(nameof(TimeStampsViewActualWidth));
                }
            }
        }

        public double TimeStampsViewActualHeight
        {
            get => _timeStampsViewActualHeight;

            set
            {
                if (value != _timeStampsViewActualHeight)
                {
                    _timeStampsViewActualHeight = value;
                    RaisePropertyChangedEvent(nameof(TimeStampsViewActualHeight));
                }
            }
        }

        public ObservableCollection<TimeStampViewElementViewModel> TimeStampsViewElements
        {
            get => _timeStampsViewElements;

            set
            {
                _timeStampsViewElements = value;
                RaisePropertyChangedEvent(nameof(TimeStampsViewElements));
            }
        }

        public double WaveShapeViewActualWidth
        {
            get => _waveShapeViewActualWidth;

            set
            {
                if (value != _waveShapeViewActualWidth)
                {
                    _waveShapeViewActualWidth = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewActualWidth));
                }
            }
        }

        public double WaveShapeViewActualHeight
        {
            get => _waveShapeViewActualHeight;

            set
            {
                if (value != _waveShapeViewActualHeight)
                {
                    _waveShapeViewActualHeight = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewActualHeight));
                }
            }
        }

        public double WaveShapeViewHorizontalOffsetSeconds
        {
            get => _waveShapeViewHorizontalOffsetSeconds;

            set
            {
                if (value != _waveShapeViewHorizontalOffsetSeconds)
                {
                    _waveShapeViewHorizontalOffsetSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewHorizontalOffsetSeconds));
                }
            }
        }

        public double WaveShapeViewWidthSeconds
        {
            get => _waveShapeViewWidthSeconds;

            set
            {
                if (_waveShapeViewWidthSeconds != value)
                {
                    _waveShapeViewWidthSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewWidthSeconds));
                }
            }
        }
        public double WaveShapeViewPixelsPerSeconds
        {
            get
            {
                var value = Settings.Default.WaveShapeViewPixelsPerSeconds;
                var normalizedValue = NormalizePerSeconsaValue(value);
                if (normalizedValue != value)
                {
                    Settings.Default.WaveShapeViewPixelsPerSeconds = normalizedValue;
                    Settings.Default.Save();
                }

                return normalizedValue;
            }

            set
            {
                var normalizedValue = NormalizePerSeconsaValue(value);
                if (normalizedValue != Settings.Default.WaveShapeViewPixelsPerSeconds)
                {
                    Settings.Default.WaveShapeViewPixelsPerSeconds = normalizedValue;
                    Settings.Default.Save();
                    RaisePropertyChangedEvent(nameof(WaveShapeViewPixelsPerSeconds));
                    RaisePropertyChangedEvent(nameof(WaveShapeViewVerticalLineTickness));
                }
            }
        }

        public double WaveShapeViewPixelsPerSampleData
        {
            get => _waveShapeViewPixelsPerSampleData;

            set
            {
                if (value != _waveShapeViewPixelsPerSampleData)
                {
                    _waveShapeViewPixelsPerSampleData = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewPixelsPerSampleData));
                    RaisePropertyChangedEvent(nameof(WaveShapeViewHorizontalLineTickness));
                }
            }
        }

        public double WaveShapeViewHorizontalLineTickness => 2.0 / WaveShapeViewPixelsPerSampleData;

        public double WaveShapeViewVerticalLineTickness => 2.0 / WaveShapeViewPixelsPerSeconds;

        public ObservableCollection<WaveShapeViewGridLineViewModel> WaveShapeViewGridLines
        {
            get => _waveShapeViewGridLines;

            set
            {
                _waveShapeViewGridLines = value;
                RaisePropertyChangedEvent(nameof(WaveShapeViewGridLines));
            }
        }

        public double WaveShapeViewHorizontalOffsetPixels
        {
            get => _waveShapeViewHorizontalOffsetPixels;

            set
            {
                if (value != _waveShapeViewHorizontalOffsetPixels)
                {
                    _waveShapeViewHorizontalOffsetPixels = value;
                    RaisePropertyChangedEvent(nameof(WaveShapeViewHorizontalOffsetPixels));
                }
            }
        }

        public PointCollection WaveShapePoints
        {
            get => _waveShapePoints;

            set
            {
                _waveShapePoints = value;
                RaisePropertyChangedEvent(nameof(WaveShapePoints));
            }
        }

        public static string FormatTime(TimeSpan time)
            => $"{(int)Math.Floor(time.TotalMinutes):D2}:{time.Seconds:D2}.{(int)Math.Round(time.Milliseconds / 10.0, 0, MidpointRounding.ToEven):D2}";

        private static TimeSpan NormalizeTime(TimeSpan value)
            => TimeSpan.FromSeconds(Math.Round(value.TotalSeconds, 2, MidpointRounding.ToEven));

        private static double NormalizePerSeconsaValue(double originalValue)
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
