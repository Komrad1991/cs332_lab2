using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using cs332_lab2.Services;
using SixLabors.ImageSharp.PixelFormats;
using Avalonia.Controls;
using AvaloniaImage = Avalonia.Controls.Image;
using SixLabors.ImageSharp;

namespace cs332_lab2.Views;

public partial class MainWindow : Window
{
    private Image<Rgba32>? _original;
    private Image<Rgba32>? _hsvResult;

    public string FileName => _fileName;
    private string _fileName = "No image loaded";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        UpdateHsvLabels();
    }

    private async void OpenImage_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp" } }
            }
        });

        if (files.Count == 0) return;

        await using var stream = await files[0].OpenReadAsync();
        _original?.Dispose();
        _original = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(stream);
        _fileName = files[0].Name;
        FileNameText.Text = _fileName;
        ProcessAll();
        ResetHsv(null, null);
    }

    private void ProcessAll()
    {
        if (_original is null) return;

        SetImage(OriginalImage, _original);

        using var gray1 = ImageProcessing.ToGray(_original, ImageProcessing.Formula601);
        using var gray2 = ImageProcessing.ToGray(_original, ImageProcessing.Formula709);
        using var diff = ImageProcessing.Difference(gray1, gray2);

        SetImage(Gray1Image, gray1);
        SetImage(Gray2Image, gray2);
        SetImage(DifferenceImage, diff);
        Gray1Histogram.Values = ImageProcessing.Histogram(gray1);
        Gray2Histogram.Values = ImageProcessing.Histogram(gray2);

        using var red = ImageProcessing.ExtractChannel(_original, 'R');
        using var green = ImageProcessing.ExtractChannel(_original, 'G');
        using var blue = ImageProcessing.ExtractChannel(_original, 'B');
        SetImage(RedImage, red);
        SetImage(GreenImage, green);
        SetImage(BlueImage, blue);
        RedHistogram.Values = ImageProcessing.Histogram(_original, 'R');
        GreenHistogram.Values = ImageProcessing.Histogram(_original, 'G');
        BlueHistogram.Values = ImageProcessing.Histogram(_original, 'B');

        UpdateHsvPreview();
    }

    private void HsvSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_original is null) return;
        UpdateHsvLabels();
        UpdateHsvPreview();
    }

    private void UpdateHsvPreview()
    {
        if (_original is null) return;
        _hsvResult?.Dispose();
        _hsvResult = ImageProcessing.AdjustHsv(
            _original,
            HueSlider.Value,
            SaturationSlider.Value,
            ValueSlider.Value);
        SetImage(HsvImage, _hsvResult);
    }

    private void UpdateHsvLabels()
    {
        HueValueText.Text = $"{HueSlider?.Value ?? 0:0}°";
        SaturationValueText.Text = $"{(SaturationSlider?.Value ?? 0):P0}";
        ValueValueText.Text = $"{(ValueSlider?.Value ?? 0):P0}";
    }

    private void ResetHsv_Click(object? sender, RoutedEventArgs e) => ResetHsv(sender, e);

    private void ResetHsv(object? sender, RoutedEventArgs? e)
    {
        HueSlider.Value = 0;
        SaturationSlider.Value = 0;
        ValueSlider.Value = 0;
        UpdateHsvLabels();
        UpdateHsvPreview();
    }

    private async void SaveHsv_Click(object? sender, RoutedEventArgs e)
    {
        if (_hsvResult is null) return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save HSV result",
            SuggestedFileName = "hsv-result.png",
            DefaultExtension = "png",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNG image") { Patterns = new[] { "*.png" } },
                new FilePickerFileType("JPEG image") { Patterns = new[] { "*.jpg", "*.jpeg" } }
            }
        });

        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        await _hsvResult.SaveAsPngAsync(stream);
    }

    private static void SetImage(AvaloniaImage target, Image<Rgba32> source) =>
        SetImageCore(target, source);

    private static void SetImage(AvaloniaImage target, Image<L8> source) =>
        SetImageCore(target, source);

    private static void SetImageCore(AvaloniaImage target, SixLabors.ImageSharp.Image source)
    {
        using var ms = new MemoryStream();
        source.SaveAsPng(ms);
        ms.Position = 0;
        target.Source = new Bitmap(ms);
    }

}
