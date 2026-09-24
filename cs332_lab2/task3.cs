using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace cs332_lab2
{
    internal class task3
    {
        public struct HSV
        {
            public double H;
            public double S;
            public double V;
        }

        public static HSV RgbToHsv(Color rgb)
        {
            double r = rgb.R / 255.0;
            double g = rgb.G / 255.0;
            double b = rgb.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;
            if (delta != 0)
            {
                if (max == r)
                    h = 60 * (((g - b) / delta) % 6);
                else if (max == g)
                    h = 60 * (((b - r) / delta) + 2);
                else if (max == b)
                    h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;

            double s = (max == 0) ? 0 : delta / max;
            double v = max;

            return new HSV { H = h, S = s, V = v };
        }

        public static Color HsvToRgb(HSV hsv)
        {
            double c = hsv.V * hsv.S;
            double x = c * (1 - Math.Abs((hsv.H / 60) % 2 - 1));
            double m = hsv.V - c;

            double rPrime = 0, gPrime = 0, bPrime = 0;

            if (hsv.H >= 0 && hsv.H < 60) { rPrime = c; gPrime = x; bPrime = 0; }
            else if (hsv.H >= 60 && hsv.H < 120) { rPrime = x; gPrime = c; bPrime = 0; }
            else if (hsv.H >= 120 && hsv.H < 180) { rPrime = 0; gPrime = c; bPrime = x; }
            else if (hsv.H >= 180 && hsv.H < 240) { rPrime = 0; gPrime = x; bPrime = c; }
            else if (hsv.H >= 240 && hsv.H < 300) { rPrime = x; gPrime = 0; bPrime = c; }
            else if (hsv.H >= 300 && hsv.H < 360) { rPrime = c; gPrime = 0; bPrime = x; }

            int r = Clamp((int)((rPrime + m) * 255));
            int g = Clamp((int)((gPrime + m) * 255));
            int b = Clamp((int)((bPrime + m) * 255));

            return Color.FromArgb(r, g, b);
        }

        private static int Clamp(int value) => Math.Max(0, Math.Min(255, value));

        public static void ModifyAndSaveHsv(string inputPath, string outputPath, double hueDelta, double satMultiplier, double valMultiplier)
        {
            using (Bitmap bmp = new Bitmap(inputPath))
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixelColor = bmp.GetPixel(x, y);
                        HSV hsv = RgbToHsv(pixelColor);

                        hsv.H = (hsv.H + hueDelta) % 360;
                        if (hsv.H < 0) hsv.H += 360;

                        hsv.S = ClampDouble(hsv.S * satMultiplier, 0.0, 1.0);
                        hsv.V = ClampDouble(hsv.V * valMultiplier, 0.0, 1.0);

                        Color newColor = HsvToRgb(hsv);
                        bmp.SetPixel(x, y, newColor);
                    }
                }
                bmp.Save(outputPath);
            }
        }
        private static double ClampDouble(double val, double min, double max) => Math.Max(min, Math.Min(max, val));

        public static void Run(string inputPath, double hueDelta = 30.0, double satMultiplier = 1.2, double valMultiplier = 1.0)
        {
            using (Bitmap bmp = new Bitmap(inputPath))
            {

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixelColor = bmp.GetPixel(x, y);
                        HSV hsv = RgbToHsv(pixelColor);

                        hsv.H = (hsv.H + hueDelta) % 360;
                        if (hsv.H < 0) hsv.H += 360;

                        hsv.S = ClampDouble(hsv.S * satMultiplier, 0.0, 1.0);
                        hsv.V = ClampDouble(hsv.V * valMultiplier, 0.0, 1.0);

                        Color newColor = HsvToRgb(hsv);
                        bmp.SetPixel(x, y, newColor);
                    }
                }

                bmp.Save("hsv_modified.jpg");
                Console.WriteLine("Результат сохранен: hsv_modified.jpg");
            }
        }
    }
}
