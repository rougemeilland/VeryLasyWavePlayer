using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using WavePlayer.GUI.Properties;
using System.Windows.Controls;

namespace WavePlayer.GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private const string _developersUrl = "https://github.com/rougemeilland/VeryLazyWavePlayer";
        private const double _pixelsPerSecondsScaleFactor = 4.0 / 3;
        private readonly MainWindowViewModel _viewModel;
        private readonly DispatcherTimer _10msecIntervalTimer;
        private DateTime? _timeToHidePopup;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel
            {
                MusicPlayingStatus = MusicPlayingStatusType.None,
                MarkedTime = TimeSpan.Zero,
                PlayingTime = TimeSpan.Zero,
            };

            _10msecIntervalTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            _10msecIntervalTimer.Tick +=
                (s, o) =>
                {
                    switch (_viewModel.MusicPlayingStatus)
                    {
                        case MusicPlayingStatusType.Playing:
                            _viewModel.PlayingTime = MusicPlayer.Position;
                            break;
                        case MusicPlayingStatusType.PlayingWithMarkerMovement:
                        case MusicPlayingStatusType.Stepping:
                            _viewModel.PlayingTime = MusicPlayer.Position;
                            _viewModel.MarkedTime = MusicPlayer.Position;
                            break;
                        default:
                            break;
                    }

                    if (_timeToHidePopup.HasValue)
                    {
                        var now = DateTime.UtcNow;
                        if (now > _timeToHidePopup.Value)
                            _viewModel.CopiedMarkedTime = false;
                    }
                };
            Closing += (s, o) => _10msecIntervalTimer.Stop();

            _viewModel.OpenCommand =
                new Command(
                    _ => CanOpenWaveFile(),
                    _ =>
                    {
                        var dialog = new OpenFileDialog();
                        var latestOpenedFilePath = _viewModel.LatestOpenedMusicFilePath;
                        if (string.IsNullOrEmpty(latestOpenedFilePath))
                        {
                            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                        }
                        else
                        {
                            dialog.InitialDirectory = Path.GetDirectoryName(latestOpenedFilePath);
                            dialog.FileName = latestOpenedFilePath;
                        }

                        dialog.Title = Properties.Resources.WINDOW_TITLE_OPEN;
                        dialog.DefaultExt = ".wav";
                        dialog.Filter = Properties.Resources.OPEN_FILE_FILTER_WAVE;
                        var result = dialog.ShowDialog();
                        if (result == true)
                        {
                            _viewModel.LatestOpenedMusicFilePath = dialog.FileName;
                            _viewModel.CurrentMusicFilePath = dialog.FileName;
                        }
                    });
            _viewModel.ExitCommand =
                new Command(_ => Close());
            _viewModel.OptionCommand =
                new Command(
                    _ =>
                    {
                        var dialog = new OptionDialogWindow
                        {
                            Owner = this,
                        };
                        var viewModel = new OptionDialogWindowViewModel
                        {
                            CultureSelectionItems =
                                new[]
                                {
                                    new ComboBoxSelectionItem<string>(Properties.Resources.SELECTION_TEXT_LANGUAGE_DEFAULT, Settings.Default.LanguageCodeDefault),
                                    new ComboBoxSelectionItem<string>(Properties.Resources.SELECTION_TEXT_LANGUAGE_ENGLISH, Settings.Default.LanguageCodeEnglish),
                                    new ComboBoxSelectionItem<string>(Properties.Resources.SELECTION_TEXT_LANGUAGE_JAPANESE, Settings.Default.LanguageCodeJapanese),
                                },
                            OkCommand =
                                new Command(
                                    p =>
                                    {
                                        dialog.DialogResult = true;
                                        dialog.Close();
                                    }),
                            CancelCommand =
                                new Command(
                                    p =>
                                    {
                                        dialog.DialogResult = false;
                                        dialog.Close();
                                    }),
                        };
                        dialog.DataContext = viewModel;
                        dialog.SelectedCulture = _viewModel.CurrentCultureName;
                        var result = dialog.ShowDialog();
                        if (result == true)
                            _viewModel.CurrentCultureName = dialog.SelectedCulture;
                    });
            _viewModel.AboutCommand =
                new Command(
                    _ =>
                    {
                        var dialog = new AboutDialogWindow
                        {
                            Owner = this,
                        };
                        var viewModel = new AboutDialogWindowViewModel
                        {
                            Version = $"Very Lazy Wave Player {GetType().Assembly.GetName().Version}",
                            Copyright = ((AssemblyCopyrightAttribute)GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright,
                            Url = _developersUrl,
                            OkCommand =
                                new Command(
                                    p => dialog.Close()),
                        };
                        dialog.DataContext = viewModel;
                        _ = dialog.ShowDialog();
                    });
            _viewModel.Play10msecAndPauseCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                        _ = Task.Run(async () =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MusicPlayer.Pause();
                                MusicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Stepping;
                                MusicPlayer.Play();
                            });
                            await Task.Delay(TimeSpan.FromMilliseconds(10));
                            Dispatcher.Invoke(() =>
                            {
                                MusicPlayer.Pause();
                                _viewModel.MarkedTime += TimeSpan.FromMilliseconds(10);
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                            });
                        }));
            _viewModel.PlayAndMoveMarkerOrPauseCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Ready:
                            case MusicPlayingStatusType.Paused:
                                MusicPlayer.Pause();
                                MusicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                MusicPlayer.Play();
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.PlayingWithMarkerMovement;
                                break;
                            case MusicPlayingStatusType.Playing:
                                MusicPlayer.Pause();
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                MusicPlayer.Pause();
                                var position = MusicPlayer.Position;
                                _viewModel.MarkedTime = position;
                                _viewModel.PlayingTime = position;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            default:
                                break;
                        }
                    });
            _viewModel.PlayFromMarkerCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Ready:
                            case MusicPlayingStatusType.Paused:
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                            case MusicPlayingStatusType.Playing:
                                MusicPlayer.Pause();
                                MusicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                MusicPlayer.Play();
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Playing;
                                break;
                            default:
                                break;
                        }
                    });
            _viewModel.PauseCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Ready:
                            case MusicPlayingStatusType.Paused:
                            case MusicPlayingStatusType.Playing:
                                MusicPlayer.Pause();
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                MusicPlayer.Pause();
                                var position = MusicPlayer.Position;
                                _viewModel.MarkedTime = position;
                                _viewModel.PlayingTime = position;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            default:
                                break;
                        }
                    });
            _viewModel.PositionForward100msecCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromMilliseconds(100)));
            _viewModel.PositionBackward100msecCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromMilliseconds(-100)));
            _viewModel.PositionForward1secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(1)));
            _viewModel.PositionBackward1secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(-1)));
            _viewModel.PositionForward10secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(10)));
            _viewModel.PositionBackward10secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(-10)));
            _viewModel.PositionForward60secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(60)));
            _viewModel.PositionBackward60secCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p => MovePlayerPosition(TimeSpan.FromSeconds(-60)));
            _viewModel.PositionHomeCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Ready:
                            case MusicPlayingStatusType.Paused:
                                MusicPlayer.Pause();
                                _viewModel.MarkedTime = TimeSpan.Zero;
                                MusicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                break;
                            case MusicPlayingStatusType.Playing:
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                MusicPlayer.Pause();
                                _viewModel.MarkedTime = TimeSpan.Zero;
                                MusicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                MusicPlayer.Play();
                                break;
                            default:
                                break;
                        }
                    });
            _viewModel.VolumeUpCommand
                = new Command(
                    p => AddVolume(10));
            _viewModel.VolumeDownCommand
                = new Command(
                    p => AddVolume(-10));
            _viewModel.CopyMarkerTextCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        // Ignore “System.Runtime.InteropServices.COMException” when operating the clipboard in WPF.
                        // See http://shen7113.blog.fc2.com/blog-entry-28.html .
                        try
                        {
                            Clipboard.SetText($"[{_viewModel.MarkedTimeText}]");
                        }
                        catch (COMException)
                        {
                        }

                        _viewModel.CopiedMarkedTime = false;
                        _viewModel.CopiedMarkedTime = true;
                        _timeToHidePopup = DateTime.UtcNow + TimeSpan.FromSeconds(4);
                    });
            _viewModel.ExpandTimeLineCommand =
                new Command(
                    p => _viewModel.WaveShapeViewPixelsPerSeconds *= _pixelsPerSecondsScaleFactor);
            _viewModel.ShrinkTimeLineCommand =
                new Command(
                    p => _viewModel.WaveShapeViewPixelsPerSeconds /= _pixelsPerSecondsScaleFactor);

            _viewModel.PropertyChanged +=
                    (s, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(_viewModel.CurrentMusicFilePath):
                                if (!string.IsNullOrEmpty(_viewModel.CurrentMusicFilePath))
                                {
                                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Loading;
                                    _viewModel.MarkedTime = TimeSpan.Zero;
                                    _viewModel.PlayingTime = TimeSpan.Zero;
                                    MusicPlayer.Pause();
                                    MusicPlayer.Volume = _viewModel.PlayerVolume / 100.0;
                                    MusicPlayer.Source = new Uri(_viewModel.CurrentMusicFilePath);
                                    MusicPlayer.Position = _viewModel.MarkedTime;
                                    _ = Task.Run(
                                        () =>
                                        {
                                            try
                                            {
                                                var waveFileBytes = File.ReadAllBytes(_viewModel.CurrentMusicFilePath);
                                                _ = Dispatcher.Invoke(() => _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Analyzing);
                                                var sampleData = SampleDataCollection.Analyze(waveFileBytes);
                                                Dispatcher.Invoke(() =>
                                                {
                                                    _viewModel.MusicDuration = sampleData.Duration;
                                                    _viewModel.WaveShapePoints = GetWaveShapePollygonPoints(sampleData);
                                                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Ready;
                                                });
                                            }
                                            catch (Exception)
                                            {
                                                _ = Dispatcher.Invoke(() => _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Error);
                                            }
                                        });
                                }

                                break;
                            case nameof(_viewModel.CurrentCultureName):
                                ResourceService.Current.ChangeCulture(_viewModel.CurrentCultureName);
                                break;
                            case nameof(_viewModel.MusicPlayingStatus):
                                _viewModel.OpenCommand.RaiseCanExecuteChanged();
                                _viewModel.Play10msecAndPauseCommand.RaiseCanExecuteChanged();
                                _viewModel.PlayAndMoveMarkerOrPauseCommand.RaiseCanExecuteChanged();
                                _viewModel.PlayFromMarkerCommand.RaiseCanExecuteChanged();
                                _viewModel.PauseCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionForward100msecCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionBackward100msecCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionForward1secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionBackward1secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionForward10secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionBackward10secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionForward60secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionBackward60secCommand.RaiseCanExecuteChanged();
                                _viewModel.PositionHomeCommand.RaiseCanExecuteChanged();
                                _viewModel.CopyMarkerTextCommand.RaiseCanExecuteChanged();
                                break;
                            default:
                                break;
                        }
                    };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.OverViewActualWidth):
                            _viewModel.OverViewMagnification = _viewModel.OverViewActualWidth / _viewModel.MusicDurationSeconds;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MarkedTimeSeconds):
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.OverViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            _viewModel.OverViewMarkedRangeLeftPixels =
                                (_viewModel.MarkedTimeSeconds - _viewModel.WaveShapeViewActualWidth / 2 / _viewModel.WaveShapeViewPixelsPerSeconds)
                                / _viewModel.MusicDurationSeconds
                                * _viewModel.OverViewActualWidth;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.WaveShapeViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.OverViewActualWidth):
                            _viewModel.OverViewMarkedRangeWidthPixels =
                                _viewModel.WaveShapeViewActualWidth
                                / _viewModel.WaveShapeViewPixelsPerSeconds
                                / _viewModel.MusicDurationSeconds
                                * _viewModel.OverViewActualWidth;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.OverViewActualWidth):
                            _viewModel.OverViewWidthPixels = _viewModel.OverViewActualWidth;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MarkedTimeSeconds):
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.OverViewActualWidth):
                            _viewModel.OverViewMarkedTimePixels = _viewModel.MarkedTimeSeconds / _viewModel.MusicDurationSeconds * _viewModel.OverViewActualWidth;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.PlayingTimeSeconds):
                        case nameof(_viewModel.OverViewActualWidth):
                            _viewModel.OverViewPlayingTimePixels = _viewModel.PlayingTimeSeconds / _viewModel.MusicDurationSeconds * _viewModel.OverViewActualWidth;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MusicDuration):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            _viewModel.TimeStampsViewElements =
                                GetTimeStamps(_viewModel.MusicDuration, _viewModel.WaveShapeViewPixelsPerSeconds, time => MainWindowViewModel.FormatTime(time));
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MusicDuration):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                        case nameof(_viewModel.WaveShapeViewVerticalLineTickness):
                            _viewModel.WaveShapeViewGridLines =
                                GetGridLines(_viewModel.MusicDuration, _viewModel.WaveShapeViewPixelsPerSeconds, _viewModel.WaveShapeViewVerticalLineTickness);
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MarkedTimeSeconds):
                        case nameof(_viewModel.TimeStampsViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            _viewModel.WaveShapeViewHorizontalOffsetPixels =
                                _viewModel.TimeStampsViewActualWidth / 2
                                - _viewModel.MarkedTimeSeconds * _viewModel.WaveShapeViewPixelsPerSeconds;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MarkedTimeSeconds):
                        case nameof(_viewModel.WaveShapeViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            _viewModel.WaveShapeViewHorizontalOffsetSeconds =
                                _viewModel.WaveShapeViewActualWidth / 2 / _viewModel.WaveShapeViewPixelsPerSeconds - _viewModel.MarkedTimeSeconds;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.WaveShapeViewActualHeight):
                            _viewModel.WaveShapeViewPixelsPerSampleData = _viewModel.WaveShapeViewActualHeight;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.WaveShapeViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            _viewModel.WaveShapeViewWidthSeconds = _viewModel.WaveShapeViewActualWidth / _viewModel.WaveShapeViewPixelsPerSeconds;
                            break;
                        default:
                            break;
                    }
                };
            _viewModel.PropertyChanged +=
                (s, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_viewModel.MusicDurationSeconds):
                        case nameof(_viewModel.WaveShapeViewActualWidth):
                        case nameof(_viewModel.WaveShapeViewPixelsPerSeconds):
                            if (_viewModel.WaveShapeViewActualWidth > 0 && _viewModel.MusicDurationSeconds > 0)
                            {
                                var limit = _viewModel.WaveShapeViewActualWidth / _viewModel.MusicDurationSeconds / 2;
                                if (_viewModel.WaveShapeViewPixelsPerSeconds < limit)
                                    _viewModel.WaveShapeViewPixelsPerSeconds = limit;
                            }

                            break;
                        default:
                            break;
                    }
                };

            DataContext = _viewModel;
            _timeToHidePopup = null;

            RecoverWindowBounds();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                _ = Task.Run(
                    () =>
                        _ = Dispatcher.Invoke(
                            () => _viewModel.CurrentMusicFilePath = args[1]));
            }

            _10msecIntervalTimer.Start();

            // TODO: 再生時にスムーズスクロールをする
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveWindowBounds();
            base.OnClosing(e);
        }

        private void SaveWindowBounds()
        {
            var settings = Settings.Default;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            WindowState = WindowState.Normal;
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.Save();
        }

        private void RecoverWindowBounds()
        {
            var settings = Settings.Default;
            if (settings.WindowLeft >= 0 &&
               (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            {
                Left = settings.WindowLeft;
            }

            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            {
                Top = settings.WindowTop;
            }

            if (settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width)
            {
                Width = settings.WindowWidth;
            }

            if (settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height)
            {
                Height = settings.WindowHeight;
            }

            if (settings.WindowMaximized)
            {
                Loaded += (o, e) => WindowState = WindowState.Maximized;
            }
        }

        private bool CanOpenWaveFile()
            => _viewModel.MusicPlayingStatus == MusicPlayingStatusType.None ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.PlayingWithMarkerMovement ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Playing ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Ready ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Paused ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Error;

        private bool CanPlayWaveFile()
            => _viewModel.MusicPlayingStatus == MusicPlayingStatusType.PlayingWithMarkerMovement ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Paused ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Ready ||
                _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Playing;

        private void MovePlayerPosition(TimeSpan value)
        {
            switch (_viewModel.MusicPlayingStatus)
            {
                case MusicPlayingStatusType.Ready:
                case MusicPlayingStatusType.Paused:
                    MusicPlayer.Pause();
                    AddMarkedTime(value);
                    MusicPlayer.Position = _viewModel.MarkedTime;
                    _viewModel.PlayingTime = _viewModel.MarkedTime;
                    break;
                case MusicPlayingStatusType.Playing:
                case MusicPlayingStatusType.PlayingWithMarkerMovement:
                    MusicPlayer.Pause();
                    AddMarkedTime(value);
                    MusicPlayer.Position = _viewModel.MarkedTime;
                    _viewModel.PlayingTime = _viewModel.MarkedTime;
                    MusicPlayer.Play();
                    break;
                default:
                    break;
            }

            void AddMarkedTime(TimeSpan time)
            {
                var newMarkedTime = _viewModel.MarkedTime + time;
                if (newMarkedTime < TimeSpan.Zero)
                    newMarkedTime = TimeSpan.Zero;
                if (newMarkedTime > _viewModel.MusicDuration)
                    newMarkedTime = _viewModel.MusicDuration;
                _viewModel.MarkedTime = newMarkedTime;
            }
        }

        private void AddVolume(int value)
        {
            var newVolume = _viewModel.PlayerVolume + value;
            if (newVolume > 100)
                newVolume = 100;
            if (newVolume < 0)
                newVolume = 0;
            _viewModel.PlayerVolume = newVolume;
            MusicPlayer.Volume = _viewModel.PlayerVolume / 100.0;
        }

        private void MoveMarkedTimeByUser(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
                time = TimeSpan.Zero;
            if (time > _viewModel.MusicDuration)
                time = _viewModel.MusicDuration;
            _viewModel.MarkedTime = time;
            _viewModel.PlayingTime = time;
            MusicPlayer.Position = time;
        }

        private static PointCollection GetWaveShapePollygonPoints(SampleDataCollection sampleData)
        {
            var points = new PointCollection();
            var lowerPoints = new Stack<Point>();
            foreach (var (time, maximumValue, minimumValue) in sampleData.EnumerateTimeLines())
            {
                points.Add(new Point(time.TotalSeconds, (1 - maximumValue) / 2));
                lowerPoints.Push(new Point(time.TotalSeconds, (1 - minimumValue) / 2));
            }

            while (lowerPoints.Count > 0)
                points.Add(lowerPoints.Pop());
            return points;
        }

        private static ObservableCollection<WaveShapeViewGridLineViewModel> GetGridLines(TimeSpan musicDuration, double pixelsPerSeconds, double verticalLineTickness)
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
                        TimeSeconds = timeSeconds,
                        Thickness = verticalLineTickness * (isBold ? 1.0 : 0.5),
                    });
            }

            return new ObservableCollection<WaveShapeViewGridLineViewModel>(gridLines);
        }

        private static ObservableCollection<TimeStampViewElementViewModel> GetTimeStamps(TimeSpan musicDuration, double pixelsPerSeconds, Func<TimeSpan, string> timeFormatter)
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
                        TimeText = timeFormatter(time),
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

        private void OverViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.OverViewActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.OverViewActualHeight = c.ActualHeight;
        }

        private void TimeStampViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.TimeStampsViewActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.TimeStampsViewActualHeight = c.ActualHeight;
        }

        private void WaveShapeViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.WaveShapeViewActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.WaveShapeViewActualHeight = c.ActualHeight;
        }

        private void OverViewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && _viewModel.OverViewMagnification > 0)
            {
                MoveMarkedTimeByUser(TimeSpan.FromSeconds(e.GetPosition(c).X / _viewModel.OverViewMagnification));
            }
        }

        private void WaveShapeViewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && _viewModel.WaveShapeViewActualWidth > 0)
            {
                MoveMarkedTimeByUser(TimeSpan.FromSeconds(e.GetPosition(c).X - _viewModel.WaveShapeViewHorizontalOffsetSeconds));
            }
        }
    }
}
