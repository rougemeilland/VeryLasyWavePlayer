using System;

namespace WavePlayer.GUI
{
    [Flags]
    internal enum AnimationMode
    {
        None = 0,
        MoveMarkerPosition = 1 << 0,
        MovePlayingPosition = 1 << 1,
    }
}
