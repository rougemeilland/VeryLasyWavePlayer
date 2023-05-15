﻿using System;
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
        private PointCollection _wavePanelWavePoints;
        private double _pixelsPerSeconds;
        private double _waveViewHorizontalOffsetSeconds;
        private double _waveViewWidthSeconds;
        private ObservableCollection<WaveViewGridLineViewModel> _waveViewGridLines;

        public Command OpenCommand { get; set; }
        public Command ExitCommand { get; set; }
        public Command OptionCommand { get; set; }
        public Command AboutCommand { get; set; }
        public Command Play10msecAndPauseCommand { get; set; }
        public Command PlayAndMoveMarkerCommand { get; set; }
        public Command PlayFromMarkerCommand { get; set; }
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
                if (value != _markedTime)
                {
                    _markedTime = value;
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
                if (value != _playingTime)
                {
                    _playingTime = value;
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

        public double WaveViewHorizontalOffsetSeconds
        {
            get => _waveViewHorizontalOffsetSeconds;

            set
            {
                if (value != _waveViewHorizontalOffsetSeconds)
                {
                    _waveViewHorizontalOffsetSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveViewHorizontalOffsetSeconds));
                }
            }
        }

        public double WaveViewWidthSeconds
        {
            get => _waveViewWidthSeconds;

            set
            {
                if (_waveViewWidthSeconds != value)
                {
                    _waveViewWidthSeconds = value;
                    RaisePropertyChangedEvent(nameof(WaveViewWidthSeconds));
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
            get
            {
                var value = Settings.Default.WavePanelPixelsPerSeconds;
                var normalizedValue = NormalizePerSeconsaValue(value);
                if (normalizedValue != value)
                    Settings.Default.WavePanelPixelsPerSeconds = normalizedValue;
                return normalizedValue;
            }

            set
            {
                var normalizedValue = NormalizePerSeconsaValue(value);
                if (normalizedValue != Settings.Default.WavePanelPixelsPerSeconds)
                {
                    Settings.Default.WavePanelPixelsPerSeconds = normalizedValue;
                    RaisePropertyChangedEvent(nameof(PixelsPerSampleData));
                    RaisePropertyChangedEvent(nameof(HorizontalLineTickness));
                }
            }
        }

        public double HorizontalLineTickness => 2.0 / PixelsPerSampleData;
        public double VerticalLineTickness => 2.0 / PixelsPerSeconds;

        public PointCollection WavePanelWavePoints
        {
            get => _wavePanelWavePoints;

            set
            {
                _wavePanelWavePoints = value;
                RaisePropertyChangedEvent(nameof(WavePanelWavePoints));
            }
        }

        public ObservableCollection<WaveViewGridLineViewModel> WaveViewGridLines
        {
            get => _waveViewGridLines;
            
            set
            {
                _waveViewGridLines = value;
                RaisePropertyChangedEvent(nameof(WaveViewGridLines));
            }
        }

        private static string FormatTime(TimeSpan time)
            => $"{(int)Math.Floor(time.TotalMinutes):D2}:{time.Seconds:D2}.{time.Milliseconds / 10:D2}";

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