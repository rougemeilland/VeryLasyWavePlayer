using System;
using System.IO;
using Palmtree.IO;
using Palmtree.Media.Wave;
using System.Media;
using System.Windows.Media;

namespace Experiment
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var data = File.ReadAllBytes(args[0]);
            var container = WaveFileContainer.Deserialize(data);
            Console.WriteLine($"{Path.GetFileName(args[0])}: totalFrames={container.TotalFrames}, channels={container.Channels}, sampleDataType={container.SampleDataType}, samplesPerSeconds={container.SamplesPerSeconds}, channelLayout={container.ChannelLayout}, validBitsPerSample={container.ValidBitsPerSample}");
            var serializedData = container.Serialize();
            for (var index = 0; index < 256; ++index)
            {
                if (index > 0 && index % 16 == 0)
                    Console.WriteLine();
                Console.Write($" {serializedData[index]:x2}");
            }

            Console.WriteLine();

            var player = new MediaPlayer();
            player.Open(new Uri(args[0]));
            player.Position = TimeSpan.FromSeconds(30);
            player.Play();

            return 0;
        }
    }
}
