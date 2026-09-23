using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace cs332_lab2
{
    internal class task2
    {
        private readonly string image_path;
        private readonly Image<Rgb24> source;

        public int[] HistogramR { get; private set; }
        public int[] HistogramG { get; private set; }
        public int[] HistogramB { get; private set; }

        public task2(string image)
        {
            image_path = image;
            source = Image.Load<Rgb24>(image_path);

            HistogramR = BuildHistogram(source, Channel.Red);
            HistogramG = BuildHistogram(source, Channel.Green);
            HistogramB = BuildHistogram(source, Channel.Blue);

            SaveChannel(source, Channel.Red,   Suffix("red"));
            SaveChannel(source, Channel.Green, Suffix("green"));
            SaveChannel(source, Channel.Blue,  Suffix("blue"));
        }

        private enum Channel { Red, Green, Blue }

        private string Suffix(string s)
        {
            string dir = Path.GetDirectoryName(image_path) ?? ".";
            string name = Path.GetFileNameWithoutExtension(image_path);
            return Path.Combine(dir, $"{name}_{s}.png");
        }

        private static void SaveChannel(Image<Rgb24> src, Channel ch, string path)
        {
            using var clone = src.Clone(ctx => ctx.ProcessPixelRowsAsVector4(row =>
            {
                for (int x = 0; x < row.Length; x++)
                {
                    var p = row[x];
                    switch (ch)
                    {
                        case Channel.Red:
                            row[x] = new System.Numerics.Vector4(p.X, 0, 0, 1);
                            break;
                        case Channel.Green:
                            row[x] = new System.Numerics.Vector4(0, p.Y, 0, 1);
                            break;
                        case Channel.Blue:
                            row[x] = new System.Numerics.Vector4(0, 0, p.Z, 1);
                            break;
                    }
                }
            }));

            clone.Save(path, new PngEncoder());
        }

        private static int[] BuildHistogram(Image<Rgb24> img, Channel ch)
        {
            var hist = new int[256];
            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        byte v = ch switch
                        {
                            Channel.Red   => row[x].R,
                            Channel.Green => row[x].G,
                            _             => row[x].B,
                        };
                        hist[v]++;
                    }
                }
            });
            return hist;
        }

        public void SaveHistogram(int[] hist, string path, SixLabors.ImageSharp.Color color,
                                  int width = 512, int height = 256)
        {
            using var bmp = new Image<Rgb24>(width, height);
            bmp.Mutate(ctx => ctx.Fill(SixLabors.ImageSharp.Color.White));

            double max = 0;
            for (int i = 0; i < 256; i++)
                if (hist[i] > 0) max = Math.Max(max, Math.Log(hist[i] + 1));
            if (max <= 0) max = 1;

            float barW = (float)width / 256f;

            bmp.Mutate(ctx =>
            {
                for (int i = 0; i < 256; i++)
                {
                    double v = hist[i] > 0 ? Math.Log(hist[i] + 1) : 0;
                    int barH = (int)(v / max * (height - 10));
                    if (barH <= 0) continue;
                    float x = i * barW;
                    ctx.Fill(color, new SixLabors.ImageSharp.RectangleF(
                        x, height - barH, Math.Max(1, barW - 1), barH));
                }
            });

            bmp.Save(path, new PngEncoder());
        }

        public static task2 Run(string imagePath)
        {
            var t = new task2(imagePath);

            string dir = Path.GetDirectoryName(imagePath) ?? ".";
            string name = Path.GetFileNameWithoutExtension(imagePath);

            t.SaveHistogram(t.HistogramR,
                Path.Combine(dir, name + "_hist_r.png"), SixLabors.ImageSharp.Color.Red);
            t.SaveHistogram(t.HistogramG,
                Path.Combine(dir, name + "_hist_g.png"), SixLabors.ImageSharp.Color.Green);
            t.SaveHistogram(t.HistogramB,
                Path.Combine(dir, name + "_hist_b.png"), SixLabors.ImageSharp.Color.Blue);

            Console.WriteLine($"Обработано: {imagePath}");
            Console.WriteLine($" Red   : {name}_red.png");
            Console.WriteLine($" Green : {name}_green.png");
            Console.WriteLine($" Blue  : {name}_blue.png");
            Console.WriteLine($" HistR : {name}_hist_r.png");
            Console.WriteLine($" HistG : {name}_hist_g.png");
            Console.WriteLine($" HistB : {name}_hist_b.png");

            return t;
        }
    }
}
