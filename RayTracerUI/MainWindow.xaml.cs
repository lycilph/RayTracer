using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayTracerUI;

public partial class MainWindow : Window
{
    const int RenderWidth = 400;
    const int RenderHeight = 225;

    WriteableBitmap _bitmap = null!;
    Renderer _renderer = null!;

    Stopwatch sw = null!;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        // WriteableBitmap must be created on the UI thread
        _bitmap = new WriteableBitmap(
            RenderWidth, RenderHeight,
            96, 96,
            PixelFormats.Rgb24,
            null);

        RenderImage.Source = _bitmap;

        _renderer = new Renderer(RenderWidth, RenderHeight,
            onRowComplete: UpdateRow,
            onComplete: OnRenderComplete);

        sw = Stopwatch.StartNew();

        // Start the renderer on a background thread —
        // never block the UI thread with heavy computation
        Task.Run(() => _renderer.Render());
    }

    // Called from the renderer for each completed row.
    // Must marshal back to the UI thread to touch the bitmap.
    void UpdateRow(int y, byte[] rowPixels, int scanlinesDone, int totalScanlines)
    {
        Dispatcher.InvokeAsync(() =>
        {
            // Write one scanline into the bitmap
            var rect = new System.Windows.Int32Rect(0, y, RenderWidth, 1);
            _bitmap.WritePixels(rect, rowPixels, RenderWidth * 3, 0);

            // Update progress
            double progress = scanlinesDone / (double)totalScanlines;
            ProgressBar.Value = progress * 100;
            StatusText.Text = $"Rendering... {scanlinesDone}/{totalScanlines} scanlines";
        });
    }

    void OnRenderComplete()
    {
        Dispatcher.InvokeAsync(() =>
        {
            ProgressBar.Value = 100;
            StatusText.Text = "Done.";
            SaveButton.IsEnabled = true;

            sw.Stop();
            TimingText.Text = "Time elapsed: " + sw.Elapsed;
        });
    }

    void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG Image|*.png",
            FileName = "render.png"
        };

        if (dialog.ShowDialog() != true) return;

        // Encode the bitmap to PNG and save
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(_bitmap));

        using var stream = File.OpenWrite(dialog.FileName);
        encoder.Save(stream);

        StatusText.Text = $"Saved → {dialog.FileName}";
    }
}