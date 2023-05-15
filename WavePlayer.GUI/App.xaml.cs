using System.Windows;

namespace WavePlayer.GUI
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
        : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            UpgradeSettings();
            base.OnStartup(e);
        }

        private static void UpgradeSettings()
        {
            var settings = GUI.Properties.Settings.Default;
            if (!settings.IsUpgraded)
            {
                settings.Upgrade();
                settings.IsUpgraded = true;
                settings.Save();
            }
        }
    }
}
