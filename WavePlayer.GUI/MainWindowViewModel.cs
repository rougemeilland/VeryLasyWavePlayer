using System;
using System.Collections.Generic;
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
        private AnimationMode _animationMode;
        private bool _isVisiblePlayView;
        private bool isValidMusicPlayingStatus;

        public MainWindowViewModel(Action<string, bool> storyboardPlayer)
        {
            OverviewView = new OverviewViewModel(this, storyboardPlayer);
            TimeStampsView = new TimeStampsViewModel(this, storyboardPlayer);
            WaveShapeView = new WaveShapeViewModel(this, storyboardPlayer);
            OverviewView.Initialize();
            TimeStampsView.Initialize();
            WaveShapeView.Initialize();

            PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(MusicPlayingStatus):
                    {
                        switch (MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Playing:
                                AnimationMode = AnimationMode.MovePlayingPosition;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_PLAYING;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = true;
                                break;
                            case MusicPlayingStatusType.Stepping:
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                AnimationMode = AnimationMode.MovePlayingPosition | AnimationMode.MoveMarkerPosition;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_PLAYING;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = true;
                                break;
                            case MusicPlayingStatusType.Ready:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_READY;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = true;
                                break;
                            case MusicPlayingStatusType.Paused:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_PAUSED;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = true;
                                break;
                            case MusicPlayingStatusType.Loading:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_LOADING;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = false;
                                break;
                            case MusicPlayingStatusType.Analyzing:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_ANALYZING;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = false;
                                break;
                            case MusicPlayingStatusType.Error:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_ERROR;
                                IsValidMusicPlayingStatus = true;
                                IsVisiblePlayView = false;
                                break;
                            default:
                                AnimationMode = AnimationMode.None;
                                MusicPlayingStatusText = Resources.STATUS_TEXT_NONE;
                                IsValidMusicPlayingStatus = false;
                                IsVisiblePlayView = false;
                                break;
                        }

                        break;
                    }
                }
            };
        }

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
                    RaisePropertyChangedEvent(nameof(MusicDurationText));
                }
            }
        }

        public string MusicDurationText => _musicDuration.FormatTime();

        public MusicPlayingStatusType MusicPlayingStatus
        {
            get => _musicPlayingStatus;

            set
            {
                if (value != _musicPlayingStatus)
                {
                    _musicPlayingStatus = value;
                    RaisePropertyChangedEvent(nameof(MusicPlayingStatus));
                }
            }
        }

        public bool IsValidMusicPlayingStatus
        {
            get => isValidMusicPlayingStatus;

            set
            {
                if (value != isValidMusicPlayingStatus)
                {
                    isValidMusicPlayingStatus = value;
                    RaisePropertyChangedEvent(nameof(isValidMusicPlayingStatus));
                }
            }
        }

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
                var normalizedTime = value.Normalize();
                if (normalizedTime != _markedTime)
                {
                    _markedTime = normalizedTime;
                    RaisePropertyChangedEvent(nameof(MarkedTime));
                    RaisePropertyChangedEvent(nameof(MarkedTimeText));
                }
            }
        }

        public string MarkedTimeText => _markedTime.FormatTime();

        public TimeSpan PlayingTime
        {
            get => _playingTime;

            set
            {
                var normalizedTime = value.Normalize();
                if (normalizedTime != _playingTime)
                {
                    _playingTime = normalizedTime;
                    RaisePropertyChangedEvent(nameof(PlayingTime));
                    RaisePropertyChangedEvent(nameof(PlayingTimeText));
                }
            }
        }

        public string PlayingTimeText => _playingTime.FormatTime();

        public bool IsVisiblePlayView
        {
            get => _isVisiblePlayView;

            set
            {
                if (value != _isVisiblePlayView)
                {
                    _isVisiblePlayView = value;
                    RaisePropertyChangedEvent(nameof(IsVisiblePlayView));
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

        public PointCollection WaveShapePoints
        {
            get => _waveShapePoints;

            set
            {
                _waveShapePoints = value;
                RaisePropertyChangedEvent(nameof(WaveShapePoints));
            }
        }

        public OverviewViewModel OverviewView { get; }
        public TimeStampsViewModel TimeStampsView { get; }
        public WaveShapeViewModel WaveShapeView { get; }

        internal AnimationMode AnimationMode
        {
            get => _animationMode;

            set
            {
                if (value != _animationMode)
                {
                    _animationMode = value;
                    RaisePropertyChangedEvent(nameof(AnimationMode));
                }
            }
        }

        internal static ObservableCollection<WaveShapeViewGridLineViewModel> GetGridLines(TimeSpan musicDuration, double pixelsPerSeconds, double actualHeight)
        {
            var (pitch, interval) = GetGridLinePitch(pixelsPerSeconds);
            var gridLines = new List<WaveShapeViewGridLineViewModel>();
            for (var index = 0; ; ++index)
            {
                var timeSeconds = pitch.TotalSeconds * index;
                if (timeSeconds >= musicDuration.TotalSeconds)
                    break;
                var isBold = index % interval == 0;
                gridLines.Add(
                    new WaveShapeViewGridLineViewModel
                    {
                        HorizontalOffsetPixels = timeSeconds * pixelsPerSeconds,
                        VerticalLengthPixels = actualHeight,
                        Thickness = isBold ? 2.0 : 1.0,
                    });
            }

            return new ObservableCollection<WaveShapeViewGridLineViewModel>(gridLines);
        }

        internal static ObservableCollection<TimeStampViewElementViewModel> GetTimeStamps(TimeSpan musicDuration, double pixelsPerSeconds)
        {
            var (pitch, interval) = GetGridLinePitch(pixelsPerSeconds);
            var timeStamps = new List<TimeStampViewElementViewModel>();
            for (var index = 0; ; index += interval)
            {
                var time = TimeSpan.FromTicks(pitch.Ticks * index);
                if (time >= musicDuration)
                    break;
                timeStamps.Add(
                    new TimeStampViewElementViewModel
                    {
                        TimeText = time.FormatTime(),
                        HorizontalPositionPixels = pixelsPerSeconds * time.TotalSeconds,
                    });
            }

            return new ObservableCollection<TimeStampViewElementViewModel>(timeStamps);
        }

        private static (TimeSpan pitch, int interval) GetGridLinePitch(double pixelsPerSeconds)
        {
            // 細線の間隔は最低でも30ピクセル以上にすること (MUST be "pitch * pixelsPerSeconds >= 30")

            if (pixelsPerSeconds >= 3000)
            {
                // 細線は10ミリ秒, 太線は50ミリ秒
                return (TimeSpan.FromMilliseconds(10), 5);
            }
            else if (pixelsPerSeconds >= 1500)
            {
                // 細線は20ミリ秒, 太線は100ミリ秒
                return (TimeSpan.FromMilliseconds(20), 5);
            }
            else if (pixelsPerSeconds >= 600)
            {
                // 細線は50ミリ秒, 太線は200ミリ秒
                return (TimeSpan.FromMilliseconds(50), 4);
            }
            else if (pixelsPerSeconds >= 300)
            {
                // 細線は100ミリ秒, 太線は500ミリ秒
                return (TimeSpan.FromMilliseconds(100), 5);
            }
            else if (pixelsPerSeconds >= 150)
            {
                // 細線は200ミリ秒, 太線は1秒
                return (TimeSpan.FromMilliseconds(200), 5);
            }
            else if (pixelsPerSeconds >= 60)
            {
                // 細線は500ミリ秒, 太線は2秒
                return (TimeSpan.FromMilliseconds(500), 4);
            }
            else if (pixelsPerSeconds >= 30)
            {
                // 細線は1秒, 太線は5秒
                return (TimeSpan.FromSeconds(1), 5);
            }
            else if (pixelsPerSeconds >= 15)
            {
                // 細線は2秒, 太線は10秒
                return (TimeSpan.FromSeconds(2), 5);
            }
            else if (pixelsPerSeconds >= 6)
            {
                // 細線は5秒, 太線は20秒
                return (TimeSpan.FromSeconds(5), 4);
            }
            else if (pixelsPerSeconds >= 3)
            {
                // 細線は10秒, 太線は1分
                return (TimeSpan.FromSeconds(10), 6);
            }
            else if (pixelsPerSeconds >= 1)
            {
                // 細線は30秒, 太線は2分
                return (TimeSpan.FromSeconds(30), 4);
            }
            else if (pixelsPerSeconds >= 0.5)
            {
                // 細線は1分, 太線は5分
                return (TimeSpan.FromMinutes(1), 5);
            }
            else if (pixelsPerSeconds >= 0.25)
            {
                // 細線は2分, 太線は10分
                return (TimeSpan.FromMinutes(2), 5);
            }
            else if (pixelsPerSeconds >= 0.1)
            {
                // 細線は5分, 太線は20分
                return (TimeSpan.FromMinutes(5), 4);
            }
            else
            {
                // 細線は10分, 太線は60分
                return (TimeSpan.FromMinutes(10), 6);
            }
        }
    }
}
