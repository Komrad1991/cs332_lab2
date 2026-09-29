using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace cs332_lab2
{
    internal class task1
    {
        private readonly string image_path;
        private readonly Image<Rgb24> source;

        public int[] Histogram1 { get; private set; }
        public int[] Histogram2 { get; private set; }

        public Image<Rgb24> Gray1 { get; private set; }
        public Image<Rgb24> Gray2 { get; private set; }
        public Image<Rgb24> Difference { get; private set; }

        /// <summary>
        /// Преобразует изображение в оттенки серого
        /// </summary>
        public task1(string image)
        {
            image_path = image;
            source = Image.Load<Rgb24>(image_path);

            Gray1 = ConvertFormula1(source);
            Gray2 = ConvertFormula2(source);

            Histogram1 = BuildHistogram(Gray1);
            Histogram2 = BuildHistogram(Gray2);

            Difference = BuildDifference(Gray1, Gray2);

            string dir = Path.GetDirectoryName(image_path) ?? ".";
            string name = Path.GetFileNameWithoutExtension(image_path);
            Gray1.SaveAsPng(Path.Combine(dir, name + "_gray1.png"));
            Gray2.SaveAsPng(Path.Combine(dir, name + "_gray2.png"));
            Difference.SaveAsPng(Path.Combine(dir, name + "_diff.png"));
        }

        /// <summary>
        /// Формула 1: Y = 0.299*R + 0.587*G + 0.114*B
        /// </summary>
        private Image<Rgb24> ConvertFormula1(Image<Rgb24> src) =>
            ConvertCore(src, 0.299, 0.587, 0.114);

        /// <summary>
        /// Формула 2: Y = 0.2126*R + 0.7152*G + 0.0722*B
        /// </summary>
        private Image<Rgb24> ConvertFormula2(Image<Rgb24> src) =>
            ConvertCore(src, 0.2126, 0.7152, 0.0722);

        private Image<Rgb24> ConvertCore(Image<Rgb24> src, double wr, double wg, double wb)
        {
            var dst = new Image<Rgb24>(src.Width, src.Height);

            src.ProcessPixelRows(dst, (srcAcc, dstAcc) =>
            {
                for (int y = 0; y < srcAcc.Height; y++)
                {
                    var sRow = srcAcc.GetRowSpan(y);
                    var dRow = dstAcc.GetRowSpan(y);

                    for (int x = 0; x < sRow.Length; x++)
                    {
                        var p = sRow[x];
                        int yVal = (int)Math.Round(wr * p.R + wg * p.G + wb * p.B);
                        if (yVal < 0) yVal = 0;
                        if (yVal > 255) yVal = 255;
                        byte yb = (byte)yVal;

                        dRow[x] = new Rgb24(yb, yb, yb);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Построение гистограммы по каналу R (изображение уже серое)
        /// </summary>
        private int[] BuildHistogram(Image<Rgb24> gray)
        {
            var hist = new int[256];

            gray.ProcessPixelRows(acc =>
            {
                for (int y = 0; y < acc.Height; y++)
                {
                    var row = acc.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                        hist[row[x].R]++;
                }
            });

            return hist;
        }

        /// <summary>
        /// Разность двух полутоновых изображений
        /// </summary>
        private Image<Rgb24> BuildDifference(Image<Rgb24> a, Image<Rgb24> b)
        {
            var dst = a.Clone();

            dst.ProcessPixelRows(b, (dAcc, bAcc) =>
            {
                for (int y = 0; y < dAcc.Height; y++)
                {
                    var dRow = dAcc.GetRowSpan(y);
                    var bRow = bAcc.GetRowSpan(y);

                    for (int x = 0; x < dRow.Length; x++)
                    {
                        int diff = Math.Abs(dRow[x].R - bRow[x].R);
                        byte dv = (byte)diff;
                        dRow[x] = new Rgb24(dv, dv, dv);
                    }
                }
            });

            return dst;
        }

        /// <summary>
        /// Сохранение гистограммы в виде PNG
        /// </summary>
        public void SaveHistogram(int[] hist, string path, int width = 512, int height = 256)
        {
            using var bmp = new Image<Rgb24>(width, height);

            double max = 0;
            for (int i = 0; i < 256; i++)
                if (hist[i] > 0) max = Math.Max(max, Math.Log(hist[i] + 1));

            if (max <= 0) max = 1;

            float barW = (float)width / 256f;
            var white = new Rgb24(255, 255, 255);
            var black = new Rgb24(0, 0, 0);

            bmp.ProcessPixelRows(acc =>
            {
                // Заливаем белым
                for (int y = 0; y < height; y++)
                    acc.GetRowSpan(y).Fill(white);

                // Рисуем чёрные столбцы
                for (int i = 0; i < 256; i++)
                {
                    double v = hist[i] > 0 ? Math.Log(hist[i] + 1) : 0;
                    int barH = (int)(v / max * (height - 10));

                    int x0 = (int)(i * barW);
                    int x1 = (int)((i + 1) * barW);
                    if (x1 <= x0) x1 = x0 + 1;
                    if (x1 > width) x1 = width;

                    int yStart = height - barH;
                    for (int y = yStart; y < height; y++)
                    {
                        var row = acc.GetRowSpan(y);
                        for (int x = x0; x < x1; x++)
                            row[x] = black;
                    }
                }
            });

            bmp.SaveAsPng(path);
        }

        /// <summary>
        /// Удобный запуск для дебага
        /// </summary>
        public static task1 Run(string imagePath)
        {
            var t = new task1(imagePath);

            string dir = Path.GetDirectoryName(imagePath) ?? ".";
            string name = Path.GetFileNameWithoutExtension(imagePath);

            t.SaveHistogram(t.Histogram1, Path.Combine(dir, name + "_hist1.png"));
            t.SaveHistogram(t.Histogram2, Path.Combine(dir, name + "_hist2.png"));

            Console.WriteLine($"Обработано: {imagePath}");
            Console.WriteLine($"  Gray1: {name}_gray1.png");
            Console.WriteLine($"  Gray2: {name}_gray2.png");
            Console.WriteLine($"  Diff : {name}_diff.png");
            Console.WriteLine($"  Hist1: {name}_hist1.png");
            Console.WriteLine($"  Hist2: {name}_hist2.png");

            return t;
        }
    }
}