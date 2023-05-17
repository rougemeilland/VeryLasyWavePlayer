using System;
using System.Windows.Media;

namespace WavePlayer.GUI
{
    /// <summary>
    /// <see cref="MediaPlayer"/> のカプセルクラス。
    /// </summary>
    /// <remarks>
    /// 本来ならカプセルクラスを用意せずに直接 <see cref="MediaPlayer"/> クラスのインスタンスを操作すればいいのだが、
    /// 作者がまだよく理解できていない機能もあり、将来の修正を容易にするためにカプセルクラスとして実装した。
    /// <list type="bullet">
    /// <item>
    /// <see cref="MediaPlayer"/> ではなく <see cref="System.Windows.Controls.MediaElement"/> を使用すると以下の不具合が発生したため <see cref="MediaPlayer"/> を採用することとした。
    /// <list type="bullet">
    /// <item>
    /// <term>発生する問題</term><description>以下の条件で意図しない位置(最初)から再生が始まる。</description>
    /// </item>
    /// <item>
    /// <term>条件</term>
    /// <description>
    /// <list type="number">
    /// <item><see cref="System.Windows.Controls.MediaElement"/> コントロールがロードされ、かつ</item>
    /// <item><see cref="System.Windows.Controls.MediaElement.Position"/> プロパティに <see cref="TimeSpan.Zero"/> を設定し、かつ (この条件が必要かどうかは未確認)</item>
    /// <item><see cref="System.Windows.Controls.MediaElement.Position"/> プロパティに <see cref="TimeSpan.Zero"/> 以外を設定し、かつ</item>
    /// <item><see cref="System.Windows.Controls.MediaElement.Play"/> を実行する。</item>
    /// </list>
    /// </description>
    /// </item>
    /// </list>
    /// </item>
    /// </list>
    /// </remarks>
    internal class MusicPlayer
    {
        private readonly MediaPlayer _player;

        public MusicPlayer() => _player = new MediaPlayer();
        public void Open(Uri uri) => _player.Open(uri);
        public void Play() => _player.Play();
        public void Pause() => _player.Pause();

        public TimeSpan Position
        {
            get => _player.Position;
            set => _player.Position = value;
        }

        public double Volume
        {
            get => _player.Volume;
            set => _player.Volume = value;
        }
    }
}
