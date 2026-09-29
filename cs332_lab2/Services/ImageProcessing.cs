using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace cs332_lab2.Services;

public static class ImageProcessing
{
    // BT.601 / Rec.601 luminance coefficients.
    public static readonly (double R, double G, double B) Formula601 = (0.299, 0.587, 0.114);

    // BT.709 / Rec.709 luminance coefficients.
    public static readonly (double R, double G, double B) Formula709 = (0.2126, 0.7152, 0.0722);

    public static Image<L8> ToGray(Image<Rgba32> source, (double R, double G, double B) weights)
    {
        var result = new Image<L8>(source.Width, source.Height);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var p = source[x, y];
            var value = weights.R * p.R + weights.G * p.G + weights.B * p.B;
            result[x, y] = new L8(ClampByte(value));
        }
        return result;
    }

    public static Image<L8> Difference(Image<L8> a, Image<L8> b)
    {
        var result = new Image<L8>(a.Width, a.Height);
        for (var y = 0; y < a.Height; y++)
        for (var x = 0; x < a.Width; x++)
        {
            var d = Math.Abs(a[x, y].PackedValue - b[x, y].PackedValue);
            result[x, y] = new L8((byte)d);
        }
        return result;
    }

    public static Image<Rgba32> ExtractChannel(Image<Rgba32> source, char channel)
    {
        var result = new Image<Rgba32>(source.Width, source.Height);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var p = source[x, y];
            byte value = channel switch
            {
                'R' => p.R,
                'G' => p.G,
                'B' => p.B,
                _ => 0
            };

            result[x, y] = channel switch
            {
                'R' => new Rgba32(value, 0, 0, 255),
                'G' => new Rgba32(0, value, 0, 255),
                'B' => new Rgba32(0, 0, value, 255),
                _ => new Rgba32(value, value, value, 255)
            };
        }
        return result;
    }

    public static int[] Histogram(Image<L8> image)
    {
        var h = new int[256];
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
            h[image[x, y].PackedValue]++;
        return h;
    }

    public static int[] Histogram(Image<Rgba32> image, char channel)
    {
        var h = new int[256];
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var p = image[x, y];
            var value = channel switch
            {
                'R' => p.R,
                'G' => p.G,
                _ => p.B
            };
            h[value]++;
        }
        return h;
    }

    public static (double H, double S, double V) RgbToHsv(Rgba32 p)
    {
        var r = p.R / 255.0;
        var g = p.G / 255.0;
        var b = p.B / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var d = max - min;
        var h = 0.0;

        if (d > 1e-12)
        {
            if (max == r) h = 60.0 * (((g - b) / d) % 6.0);
            else if (max == g) h = 60.0 * ((b - r) / d + 2.0);
            else h = 60.0 * ((r - g) / d + 4.0);
            if (h < 0) h += 360.0;
        }

        var s = max <= 1e-12 ? 0 : d / max;
        return (h, s, max);
    }

    public static Rgba32 HsvToRgb(double h, double s, double v)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Clamp(s, 0, 1);
        v = Math.Clamp(v, 0, 1);

        var c = v * s;
        var x = c * (1 - Math.Abs((h / 60.0 % 2) - 1));
        var m = v - c;
        var (r, g, b) = h switch
        {
            < 60 => (c, x, 0.0),
            < 120 => (x, c, 0.0),
            < 180 => (0.0, c, x),
            < 240 => (0.0, x, c),
            < 300 => (x, 0.0, c),
            _ => (c, 0.0, x)
        };

        return new Rgba32(ClampByte((r + m) * 255), ClampByte((g + m) * 255), ClampByte((b + m) * 255), 255);
    }

    public static Image<Rgba32> AdjustHsv(Image<Rgba32> source, double hueShift, double saturationDelta, double valueDelta)
    {
        var result = new Image<Rgba32>(source.Width, source.Height);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var (h, s, v) = RgbToHsv(source[x, y]);
            h = (h + hueShift + 360) % 360;
            s = Math.Clamp(s + saturationDelta, 0, 1);
            v = Math.Clamp(v + valueDelta, 0, 1);
            result[x, y] = HsvToRgb(h, s, v);
        }
        return result;
    }

    private static byte ClampByte(double value) => (byte)Math.Clamp(Math.Round(value), 0, 255);
}
