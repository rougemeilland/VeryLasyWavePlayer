using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Experiment
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var fps = 29.97;
            var totalFrames = fps * 60;
            for (var frame = 0; frame < totalFrames; ++frame)
            {
                using (var canvas = new Bitmap(640, 480))
                using (var g = Graphics.FromImage(canvas))
                using (var fnt = new Font("MS UI Gothic", 40))
                {
                    RectangleF rect = new RectangleF(10, 10, 620, 460);
                    g.FillRectangle(Brushes.White, rect);
                    g.DrawString(
                        string.Join(
                            "\n",
                            new[]
                            {
                                $"Frame={frame,4}",
                                $"Time={frame / fps,9:F6}[sec]",
                            }),
                        fnt,
                        Brushes.Black,
                        rect);
                    canvas.Save($"frames_{frame:D4}.png", ImageFormat.Png);
                }
            }

            return 0;
        }
    }
}
