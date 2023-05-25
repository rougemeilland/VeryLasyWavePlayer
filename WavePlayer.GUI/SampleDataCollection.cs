using System;
using System.Collections.Generic;
using System.Linq;
using Palmtree;
using Palmtree.Media;
using Palmtree.Media.Wave;

namespace WavePlayer.GUI
{
    internal class SampleDataCollection
    {
        private struct TimeLineBoundary
        {
            public int Frame;
            public TimeSpan Time;
        }

        private struct TimeLine
        {
            public TimeLineBoundary Start;
            public TimeLineBoundary End;
            public double MaximumSampleData;
            public double MinimumSampleData;
        }

        private readonly TimeLine[] _timeLines;

        private SampleDataCollection(IEnumerable<TimeLine> timeLines)
        {
            _timeLines = timeLines.ToArray();
            Duration = _timeLines[_timeLines.Length - 1].End.Time;
            NormalizeTimeLines(_timeLines);
        }

        public IEnumerable<(TimeSpan time, double maximumValue, double minimumValue)> EnumerateTimeLines()
        {
            if (_timeLines.Length >= 0)
                yield return (_timeLines[0].Start.Time, 0.0, 0.0);
            foreach (var timeLine in _timeLines)
                yield return (timeLine.End.Time, timeLine.MaximumSampleData, timeLine.MinimumSampleData);
        }

        public TimeSpan Duration { get; }

#if false
        public TimeSpan GoToNextTabStop(TimeSpan time)
        {
        }

        public TimeSpan GoToPreviousTabStop(TimeSpan time)
        {
        }
#endif

        public static SampleDataCollection Analyze(ReadOnlySpan<byte> waveFileBytes)
        {
            var waveFile = WaveFileContainer.Deserialize(waveFileBytes);
            var sampleDataType = waveFile.SampleDataType;
            var channels = waveFile.Channels;
            var totalFrames = waveFile.TotalFrames;

            var timeBoundaries = new List<TimeLineBoundary>();
            for (var time = TimeSpan.Zero; ; time += TimeSpan.FromMilliseconds(10))
            {
                var frame = waveFile.GetFrameNumberFromTime(time);
                if (frame >= totalFrames)
                {
                    timeBoundaries.Add(new TimeLineBoundary { Time = waveFile.GetTimeFromFrameNumber(totalFrames), Frame = totalFrames });
                    break;
                }
                else
                {
                    timeBoundaries.Add(new TimeLineBoundary { Time = waveFile.GetTimeFromFrameNumber(frame), Frame = frame });
                }
            }

            var timeBoundaryArray = timeBoundaries.ToArray();
            var timeLines = new List<TimeLine>();
            for (var index = 0; index < timeBoundaryArray.Length - 1; index++)
            {
                var start = timeBoundaryArray[index];
                var end = timeBoundaryArray[index + 1];
                var minimumSampleData = 0.0;
                var maximumSampleData = 0.0;
                for (var frame = start.Frame; frame < end.Frame; ++frame)
                {
                    for (var channel = 0; channel < channels; ++channel)
                    {
                        var sampleData = GetSampleData(channel, frame);
                        if (minimumSampleData > sampleData)
                            minimumSampleData = sampleData;
                        if (maximumSampleData < sampleData)
                            maximumSampleData = sampleData;
                    }
                }

                timeLines.Add(new TimeLine { Start = start, End = end, MaximumSampleData = maximumSampleData, MinimumSampleData = minimumSampleData });
            }

            return new SampleDataCollection(timeLines);

            double GetSampleData(int channel, int frame)
            {
                switch (sampleDataType)
                {
                    case WaveSampleDataType.Unsigned8bit:
                        return waveFile.GetSampleDataAsU8(channel, frame) - 128;
                    case WaveSampleDataType.LittleEndianSigned16bit:
                        return waveFile.GetSampleDataAsS16LE(channel, frame);
                    case WaveSampleDataType.LittleEndianSigned24bit:
                        return waveFile.GetSampleDataAsS24LE(channel, frame);
                    case WaveSampleDataType.LittleEndianSigned32bit:
                        return waveFile.GetSampleDataAsS32LE(channel, frame);
                    case WaveSampleDataType.LittleEndianSigned64bit:
                        return waveFile.GetSampleDataAsS64LE(channel, frame);
                    case WaveSampleDataType.LittleEndian32bitFloat:
                        return waveFile.GetSampleDataAsF32LE(channel, frame);
                    case WaveSampleDataType.LittleEndian64bitFloat:
                        return waveFile.GetSampleDataAsF64LE(channel, frame);
                    default:
                        throw new NotSupportedMediaException($"Sample data of type \"{sampleDataType}\" is not supported.");
                }
            }
        }

        private static void NormalizeTimeLines(TimeLine[] timeLines)
        {
            var maximumSampleData = 0.0;
            var minimumSampleData = 0.0;
            for (var index = 0; index < timeLines.Length; index++)
            {
                var timeLine = timeLines[index];
                if (maximumSampleData < timeLine.MaximumSampleData)
                    maximumSampleData = timeLine.MaximumSampleData;
                if (minimumSampleData > timeLine.MinimumSampleData)
                    minimumSampleData = timeLine.MinimumSampleData;
            }

            var amplitude = maximumSampleData;
            if (amplitude < -minimumSampleData)
                amplitude = -minimumSampleData;
            if (amplitude > 0)
            {
                for (var index = 0; index < timeLines.Length; index++)
                {
                    timeLines[index].MaximumSampleData /= amplitude;
                    timeLines[index].MinimumSampleData /= amplitude;
                }
            }
        }
    }
}
