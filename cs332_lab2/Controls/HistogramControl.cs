using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace cs332_lab2.Controls;

public class HistogramControl : Control
{
    public static readonly StyledProperty<int[]> ValuesProperty =
        AvaloniaProperty.Register<HistogramControl, int[]>(nameof(Values), Array.Empty<int>());

    public int[] Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    static HistogramControl()
    {
        ValuesProperty.Changed.AddClassHandler<HistogramControl>((control, _) => control.InvalidateVisual());
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var rect = Bounds.Deflate(4);
        var background = new SolidColorBrush(Color.Parse("#111827"));
        context.FillRectangle(background, rect);

        if (Values.Length != 256 || rect.Width <= 0 || rect.Height <= 0) return;

        var max = Values.Max();
        if (max == 0) return;

        var pen = new Pen(new SolidColorBrush(Color.Parse("#60A5FA")), Math.Max(1, rect.Width / 256.0));
        for (var i = 0; i < 256; i++)
        {
            var x = rect.X + i * rect.Width / 255.0;
            var height = Values[i] / (double)max * (rect.Height - 8);
            context.DrawLine(pen, new Point(x, rect.Bottom - 4), new Point(x, rect.Bottom - 4 - height));
        }
    }
}
