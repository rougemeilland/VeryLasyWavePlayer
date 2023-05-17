using System;

namespace WavePlayer.GUI
{
    internal static class TimeSpanExtensions
    {
        public static string FormatTime(this TimeSpan time)
            => $"{(int)Math.Floor(time.TotalMinutes):D2}:{time.Seconds:D2}.{(int)Math.Round(time.Milliseconds / 10.0, 0, MidpointRounding.ToEven):D2}";

        public static TimeSpan Normalize(this TimeSpan value)
            => TimeSpan.FromSeconds(Math.Round(value.TotalSeconds, 2, MidpointRounding.ToEven));
    }
}
