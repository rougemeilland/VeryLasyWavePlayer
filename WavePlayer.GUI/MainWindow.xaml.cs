using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using WavePlayer.GUI.Properties;

namespace WavePlayer.GUI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private const string _developersUrl = "https://github.com/rougemeilland/VeryLazyWavePlayer";
        private const string _onlineHelpUrl = "https://github.com/rougemeilland/VeryLazyWavePlayer/tree/main#readme";
        private const double _pixelsPerSecondsScaleFactor = 4.0 / 3;
        private static readonly Regex _pastedTimePattern = new Regex(@"^\s*\[?((?<m>\d+):)?(?<s>\d+(\.\d+))?\]?\s*$", RegexOptions.Compiled);
        private readonly MainWindowViewModel _viewModel;
        private readonly MusicPlayer _musicPlayer;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel(
                (key, start) =>
                {
                    if (start)
                        ((Storyboard)Resources[key]).Begin();
                    else
                        ((Storyboard)Resources[key]).Stop();
                })
            {
                MusicPlayingStatus = MusicPlayingStatusType.None,
                MarkedTime = TimeSpan.Zero,
                PlayingTime = TimeSpan.Zero,
            };

            _musicPlayer = new MusicPlayer();

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
            _viewModel.HelpCommand =
                new Command(
                    _ => _ = Process.Start(new ProcessStartInfo { FileName = _onlineHelpUrl, UseShellExecute = true }));
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

            //
            // 【注意】再生制御イベント/ウィンドウサイズ変更イベントでしなければならないこと
            // イベント処理において以下の何れかの対処を必ず行うこと。
            // a) _viewModel.MusicPlayingStatus の値を確認し、a-1 ・ a-2の対処を行うこと。
            //   a-1) _viewModel.MusicPlayingStatus == MusicPlayingStatusType.Playing の場合
            //       _musicPlayer.Position の値を _viewModel.PlayingTime に反映させる。
            //   a-2) _viewModel.MusicPlayingStatus == MusicPlayingStatusType.PlayingWithMarkerMovement の場合
            //       _musicPlayer.Position の値を _viewModel.PlayingTime と  _viewModel.MarkedTime に反映させる。
            // b) _viewModel.AnimationMode の値を確認し、b-1 ・ b-2の対処を行うこと。
            //   b-1) (_viewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.Node の場合
            //       _musicPlayer.Position の値を _viewModel.MarkedTime に反映させる。
            //   b-2) (_viewModel.AnimationMode & AnimationMode.MovePlayingPosition) != AnimationMode.Node の場合
            //       _musicPlayer.Position の値を _viewModel.PlayingTime に反映させる。
            //
            // 【_musicPlayer.Positionの値の取得について】
            // Pause() を実行しても、即時に停止するわけではないらしい。
            // 特に画面の再描画を行っている場合などフォアグラウンドの負荷が高い状態で Pause() の直後に Position の値を続けて参照すると
            // 本来はどれも同じ値になるはずが、微妙に異なる値が取得出来てしまうことがある。
            // Position の値を続けて複数個所で参照する場合は、なるべく 1 回だけ Position に参照して、得た値を再利用すること。
            //
            _viewModel.Play10msecAndPauseCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        _musicPlayer.Pause();
                        var time = _musicPlayer.Position;
                        if ((_viewModel.AnimationMode & AnimationMode.MoveMarkerPosition) != AnimationMode.None)
                            _viewModel.MarkedTime = time;
                        if ((_viewModel.AnimationMode & AnimationMode.MovePlayingPosition) != AnimationMode.None)
                            _viewModel.PlayingTime = time;
                        _musicPlayer.Position = _viewModel.MarkedTime;
                        _viewModel.PlayingTime = _viewModel.MarkedTime;
                        _musicPlayer.Play();
                        _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Stepping;
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(10));
                            Dispatcher.Invoke(() =>
                            {
                                _musicPlayer.Pause();
                                _viewModel.MarkedTime += TimeSpan.FromMilliseconds(10);
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                            });
                        });
                    });
            _viewModel.PlayAndMoveMarkerOrPauseCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Ready:
                            case MusicPlayingStatusType.Paused:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.PlayingWithMarkerMovement;
                                break;
                            case MusicPlayingStatusType.Playing:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _musicPlayer.Position; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                            {
                                _musicPlayer.Pause();
                                var currentTime = _musicPlayer.Position;
                                _viewModel.MarkedTime = currentTime;
                                _viewModel.PlayingTime = currentTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            }
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
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Playing;
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                _musicPlayer.Pause();
                                _viewModel.MarkedTime = _musicPlayer.Position;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Playing;
                                break;
                            case MusicPlayingStatusType.Playing:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _musicPlayer.Position; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
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
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            case MusicPlayingStatusType.Playing:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _musicPlayer.Position; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                _musicPlayer.Pause();
                                var position = _musicPlayer.Position;
                                _viewModel.MarkedTime = position;
                                _viewModel.PlayingTime = position;
                                _musicPlayer.Position = _viewModel.MarkedTime;
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
                                _musicPlayer.Pause();
                                _viewModel.MarkedTime = TimeSpan.Zero;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                break;
                            case MusicPlayingStatusType.Playing:
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                                _musicPlayer.Pause();
                                _viewModel.MarkedTime = TimeSpan.Zero;
                                _viewModel.PlayingTime = _viewModel.MarkedTime;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
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

                        _viewModel.MarkedTimeClipboardAction = ClipboardActionType.Copy;
                        _viewModel.BlinkMarkedTimeText = false;
                        _viewModel.BlinkMarkedTimeText = true;
                    });
            _viewModel.PasteMarkerTextCommand
                = new Command(
                    p => CanPlayWaveFile(),
                    p =>
                    {
                        var pastedData = Clipboard.GetDataObject();
                        if (!(pastedData is null))
                        {
                            var pastedText = pastedData.GetData(DataFormats.Text, true) as string;
                            if (!string.IsNullOrEmpty(pastedText))
                            {
                                var match = _pastedTimePattern.Match(pastedText);
                                if (match.Success)
                                {
                                    var minutes = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat) : 0;
                                    var seconds = double.Parse(match.Groups["s"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                                    var pastedTime = TimeSpan.FromSeconds(minutes * 60 + seconds);
                                    if (pastedTime >= TimeSpan.Zero && pastedTime < _viewModel.MusicDuration)
                                    {
                                        _viewModel.MarkedTimeClipboardAction = ClipboardActionType.Paste;
                                        _viewModel.BlinkMarkedTimeText = false;
                                        _viewModel.BlinkMarkedTimeText = true;
                                        MoveMarkedTimeByUser(pastedTime);
                                    }
                                }
                            }
                        }
                    });
            _viewModel.ExpandTimeLineCommand =
                new Command(
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Playing:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _musicPlayer.Position;
                                _viewModel.WaveShapeView.PixelsPerSeconds *= _pixelsPerSecondsScaleFactor;
                                _musicPlayer.Position = _viewModel.PlayingTime;
                                _musicPlayer.Play();
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                            {
                                _musicPlayer.Pause();
                                var currentTime = _musicPlayer.Position;
                                _viewModel.MarkedTime = currentTime;
                                _viewModel.PlayingTime = currentTime;
                                _viewModel.WaveShapeView.PixelsPerSeconds *= _pixelsPerSecondsScaleFactor;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
                                break;
                            }
                            default:
                                _viewModel.WaveShapeView.PixelsPerSeconds *= _pixelsPerSecondsScaleFactor;
                                break;
                        }
                    });
            _viewModel.ShrinkTimeLineCommand =
                new Command(
                    p =>
                    {
                        switch (_viewModel.MusicPlayingStatus)
                        {
                            case MusicPlayingStatusType.Playing:
                                _musicPlayer.Pause();
                                _viewModel.PlayingTime = _musicPlayer.Position;
                                _viewModel.WaveShapeView.PixelsPerSeconds /= _pixelsPerSecondsScaleFactor;
                                _musicPlayer.Position = _viewModel.PlayingTime;
                                _musicPlayer.Play();
                                break;
                            case MusicPlayingStatusType.PlayingWithMarkerMovement:
                            {
                                _musicPlayer.Pause();
                                var currentTime = _musicPlayer.Position;
                                _viewModel.MarkedTime = currentTime;
                                _viewModel.PlayingTime = currentTime;
                                _viewModel.WaveShapeView.PixelsPerSeconds /= _pixelsPerSecondsScaleFactor;
                                _musicPlayer.Position = _viewModel.MarkedTime;
                                _musicPlayer.Play();
                                break;
                            }
                            default:
                                _viewModel.WaveShapeView.PixelsPerSeconds /= _pixelsPerSecondsScaleFactor;
                                break;
                        }
                    });

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
                                    _musicPlayer.Pause();
                                    _musicPlayer.Volume = _viewModel.PlayerVolume / 100.0;
                                    _musicPlayer.Open(new Uri(_viewModel.CurrentMusicFilePath));
                                    _musicPlayer.Position = _viewModel.MarkedTime;
                                    _ = Task.Run(
                                        () =>
                                        {
                                            try
                                            {
                                                var waveFileBytes = File.ReadAllBytes(_viewModel.CurrentMusicFilePath);
                                                Dispatcher.Invoke(() =>
                                                {
                                                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Analyzing;
                                                    _ = Task.Run(() =>
                                                    {
                                                        try
                                                        {
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
                                _viewModel.PasteMarkerTextCommand.RaiseCanExecuteChanged();
                                break;
                            default:
                                break;
                        }
                    };

            DataContext = _viewModel;

            RecoverWindowBounds();

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                _ = Task.Run(
                    () =>
                        _ = Dispatcher.Invoke(
                            () => _viewModel.CurrentMusicFilePath = args[1]));
            }
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
                    _musicPlayer.Pause();
                    _viewModel.MarkedTime = AddMarkedTime(_viewModel.MarkedTime, value, _viewModel.MusicDuration);
                    _viewModel.PlayingTime = _viewModel.MarkedTime;
                    _musicPlayer.Position = _viewModel.MarkedTime;
                    break;
                case MusicPlayingStatusType.Playing:
                    _musicPlayer.Pause();
                    _viewModel.PlayingTime = _musicPlayer.Position; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                    _viewModel.MarkedTime = AddMarkedTime(_viewModel.MarkedTime, value, _viewModel.MusicDuration);
                    _viewModel.PlayingTime = _viewModel.MarkedTime;
                    _musicPlayer.Position = _viewModel.MarkedTime;
                    _musicPlayer.Play();
                    break;
                case MusicPlayingStatusType.PlayingWithMarkerMovement:
                    _musicPlayer.Pause();
                    _viewModel.MarkedTime = AddMarkedTime(_musicPlayer.Position, value, _viewModel.MusicDuration);
                    _viewModel.PlayingTime = _viewModel.MarkedTime;
                    _musicPlayer.Position = _viewModel.MarkedTime;
                    _musicPlayer.Play();
                    break;
                default:
                    break;
            }

            TimeSpan AddMarkedTime(TimeSpan markedTime, TimeSpan time, TimeSpan musicDuration)
            {
                var newMarkedTime = markedTime + time;
                if (newMarkedTime < TimeSpan.Zero)
                    newMarkedTime = TimeSpan.Zero;
                if (newMarkedTime > musicDuration)
                    newMarkedTime = musicDuration;
                return newMarkedTime;
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
            _musicPlayer.Volume = _viewModel.PlayerVolume / 100.0;
        }

        private void MoveMarkedTimeByUser(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
                time = TimeSpan.Zero;
            if (time > _viewModel.MusicDuration)
                time = _viewModel.MusicDuration;
            switch (_viewModel.MusicPlayingStatus)
            {
                case MusicPlayingStatusType.Ready:
                case MusicPlayingStatusType.Paused:
                    _musicPlayer.Pause();
                    _viewModel.MarkedTime = time;
                    _viewModel.PlayingTime = time;
                    _musicPlayer.Position = time;
                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                    break;
                case MusicPlayingStatusType.Playing:
                    _musicPlayer.Pause();
                    _viewModel.PlayingTime = _musicPlayer.Position; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                    _viewModel.MarkedTime = time;
                    _viewModel.PlayingTime = time;
                    _musicPlayer.Position = time;
                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                    break;
                case MusicPlayingStatusType.PlayingWithMarkerMovement:
                {
                    _musicPlayer.Pause();
                    var currentTime = _musicPlayer.Position;
                    _viewModel.MarkedTime = currentTime; // _viewModel.MarkedTime の変更イベントを発生させるために必要なコード
                    _viewModel.PlayingTime = currentTime; // _viewModel.PlayingTime の変更イベントを発生させるために必要なコード
                    _viewModel.MarkedTime = time;
                    _viewModel.PlayingTime = time;
                    _musicPlayer.Position = time;
                    _viewModel.MusicPlayingStatus = MusicPlayingStatusType.Paused;
                    break;
                }
                default:
                    break;
            }
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

        private void SyncViewModelTimes()
        {
            switch (_viewModel.MusicPlayingStatus)
            {
                case MusicPlayingStatusType.Playing:
                    _musicPlayer.Pause();
                    _viewModel.PlayingTime = _musicPlayer.Position;
                    _musicPlayer.Position = _viewModel.PlayingTime;
                    break;
                case MusicPlayingStatusType.PlayingWithMarkerMovement:
                {
                    _musicPlayer.Pause();
                    var currentTime = _musicPlayer.Position;
                    _viewModel.MarkedTime = currentTime;
                    _viewModel.PlayingTime = currentTime;
                    _musicPlayer.Position = _viewModel.MarkedTime;
                    break;
                }
                default:
                    break;
            }
        }

        private void OverviewViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.OverviewView.ActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.OverviewView.ActualHeight = c.ActualHeight;
            SyncViewModelTimes();
        }

        private void TimeStampViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.TimeStampsView.ActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.TimeStampsView.ActualHeight = c.ActualHeight;
            SyncViewModelTimes();
        }

        private void WaveShapeViewSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.WidthChanged)
                _viewModel.WaveShapeView.ActualWidth = c.ActualWidth;
            if (e.HeightChanged)
                _viewModel.WaveShapeView.ActualHeight = c.ActualHeight;
            SyncViewModelTimes();
        }

        private void OverviewViewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && _viewModel.OverviewView.HorizontalMagnification > 0)
            {
                MoveMarkedTimeByUser(TimeSpan.FromSeconds(e.GetPosition(c).X / _viewModel.OverviewView.HorizontalMagnification));
            }
        }

        private void WaveShapeViewMouseDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            var c = (FrameworkElement)sender;
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && _viewModel.WaveShapeView.PixelsPerSeconds > 0)
            {
                MoveMarkedTimeByUser(TimeSpan.FromSeconds((e.GetPosition(c).X - _viewModel.WaveShapeView.HalfOfActualWidth) / _viewModel.WaveShapeView.PixelsPerSeconds) + _viewModel.MarkedTime);
            }
        }
    }
}
