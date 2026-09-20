using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace cs332_lab2
{
    internal class task1
    {
        private readonly string image_path;
        private readonly Bitmap source;

        public int[] Histogram1 { get; private set; }
        public int[] Histogram2 { get; private set; }

        public Bitmap Gray1 { get; private set; }
        public Bitmap Gray2 { get; private set; }
        public Bitmap Difference { get; private set; }

        /// <summary>
        /// Преобразует изображение в оттенки серого
        /// </summary>
        /// <param name="image"></param>
        public task1(string image)
        {
            image_path = image;
            source = new Bitmap(image_path);

            Gray1 = ConvertFormula1(source);
            Gray2 = ConvertFormula2(source);

            Histogram1 = BuildHistogram(Gray1);
            Histogram2 = BuildHistogram(Gray2);

            Difference = BuildDifference(Gray1, Gray2);

            
            string dir = System.IO.Path.GetDirectoryName(image_path) ?? ".";
            string name = System.IO.Path.GetFileNameWithoutExtension(image_path);
            Gray1.Save(System.IO.Path.Combine(dir, name + "_gray1.png"), ImageFormat.Png);
            Gray2.Save(System.IO.Path.Combine(dir, name + "_gray2.png"), ImageFormat.Png);
            Difference.Save(System.IO.Path.Combine(dir, name + "_diff.png"), ImageFormat.Png);
        }

        /// <summary>
        /// Формула 1
        /// Y = 0.299*R + 0.587*G + 0.114*B
        /// </summary>
        private Bitmap ConvertFormula1(Bitmap src)
        {
            return ConvertCore(src, 0.299, 0.587, 0.114);
        }

        /// <summary>
        /// Формула 2
        /// Y = 0.2126*R + 0.7152*G + 0.0722*B
        /// </summary>
        private Bitmap ConvertFormula2(Bitmap src)
        {
            return ConvertCore(src, 0.2126, 0.7152, 0.0722);
        }

        private Bitmap ConvertCore(Bitmap src, double wr, double wg, double wb)
        {
            int w = src.Width;
            int h = src.Height;
            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            var srcData = src.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = dst.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                byte[] srcBuf = new byte[srcStride * h];
                byte[] dstBuf = new byte[dstStride * h];
                Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBuf.Length);

                for (int y = 0; y < h; y++)
                {
                    int sRow = y * srcStride;
                    int dRow = y * dstStride;
                    for (int x = 0; x < w; x++)
                    {
                        int i = sRow + x * 3;
                        byte b = srcBuf[i];
                        byte g = srcBuf[i + 1];
                        byte r = srcBuf[i + 2];

                        int yVal = (int)Math.Round(wr * r + wg * g + wb * b);
                        if (yVal < 0) yVal = 0;
                        if (yVal > 255) yVal = 255;
                        byte yb = (byte)yVal;

                        int j = dRow + x * 3;
                        dstBuf[j] = yb;
                        dstBuf[j + 1] = yb;
                        dstBuf[j + 2] = yb;
                    }
                }
                Marshal.Copy(dstBuf, 0, dstData.Scan0, dstBuf.Length);
            }
            finally
            {
                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }

            return dst;
        }

        /// <summary>
        /// Построение гистограммы по каналу R
        /// </summary>
        private int[] BuildHistogram(Bitmap gray)
        {
            var hist = new int[256];
            int w = gray.Width;
            int h = gray.Height;

            var data = gray.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                int stride = data.Stride;
                byte[] buf = new byte[stride * h];
                Marshal.Copy(data.Scan0, buf, 0, buf.Length);

                for (int y = 0; y < h; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < w; x++)
                    {
                        byte v = buf[row + x * 3 + 2]; // R
                        hist[v]++;
                    }
                }
            }
            finally
            {
                gray.UnlockBits(data);
            }
            return hist;
        }

        /// <summary>
        /// Разность двух полутоновых изображений.
        /// </summary>
        private Bitmap BuildDifference(Bitmap a, Bitmap b)
        {
            int w = a.Width;
            int h = a.Height;
            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);

            var ad = a.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bd = b.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dd = dst.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte[] aBuf = new byte[ad.Stride * h];
                byte[] bBuf = new byte[bd.Stride * h];
                byte[] dBuf = new byte[dd.Stride * h];
                Marshal.Copy(ad.Scan0, aBuf, 0, aBuf.Length);
                Marshal.Copy(bd.Scan0, bBuf, 0, bBuf.Length);

                for (int y = 0; y < h; y++)
                {
                    int aRow = y * ad.Stride;
                    int bRow = y * bd.Stride;
                    int dRow = y * dd.Stride;
                    for (int x = 0; x < w; x++)
                    {
                        int va = aBuf[aRow + x * 3 + 2];
                        int vb = bBuf[bRow + x * 3 + 2];
                        int diff = Math.Abs(va - vb);
                        if (diff > 255) diff = 255;
                        byte dv = (byte)diff;

                        int k = dRow + x * 3;
                        dBuf[k] = dv;
                        dBuf[k + 1] = dv;
                        dBuf[k + 2] = dv;
                    }
                }
                Marshal.Copy(dBuf, 0, dd.Scan0, dBuf.Length);
            }
            finally
            {
                a.UnlockBits(ad);
                b.UnlockBits(bd);
                dst.UnlockBits(dd);
            }

            return dst;
        }

        /// <summary>
        /// Сохранение гистограммы в виде PNG
        /// </summary>
        public void SaveHistogram(int[] hist, string path, int width = 512, int height = 256)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                double max = 0;
                for (int i = 0; i < 256; i++)
                    if (hist[i] > 0) max = Math.Max(max, Math.Log(hist[i] + 1));

                if (max <= 0) max = 1;

                float barW = (float)width / 256f;
                using (var pen = new Pen(Color.Black))
                {
                    for (int i = 0; i < 256; i++)
                    {
                        double v = hist[i] > 0 ? Math.Log(hist[i] + 1) : 0;
                        int barH = (int)(v / max * (height - 10));
                        float x = i * barW;
                        g.DrawRectangle(pen, x, height - barH, Math.Max(1, barW - 1), barH);
                    }
                }
            }
            bmp.Save(path, ImageFormat.Png);
        }

        /// <summary>
        /// Удобный запуск для дебага
        /// </summary>
        public static task1 Run(string imagePath)
        {
            var t = new task1(imagePath);

            string dir = System.IO.Path.GetDirectoryName(imagePath) ?? ".";
            string name = System.IO.Path.GetFileNameWithoutExtension(imagePath);

            t.SaveHistogram(t.Histogram1, System.IO.Path.Combine(dir, name + "_hist1.png"));
            t.SaveHistogram(t.Histogram2, System.IO.Path.Combine(dir, name + "_hist2.png"));

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