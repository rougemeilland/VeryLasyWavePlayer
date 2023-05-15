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
            for (int i = 0; i < 23; ++i)
            {
                var x1 = 0.001 * i;
                var x2 = Math.Round(x1, 2, MidpointRounding.ToEven);
                Console.WriteLine($"x1={x1:F4}, x2={x2:F4}");
            }

            return 0;
        }
    }
}
