using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WriteableBitmapTest
{
    public partial class MainWindow
    {
        private WriteableBitmap bmp;
        private byte[] pixels;

        public MainWindow()
        {
            InitializeComponent();

            bmp = new WriteableBitmap(300, 300, 96, 96, PixelFormats.Rgb24, null);
            pixels = new byte[bmp.PixelHeight * bmp.PixelWidth * bmp.Format.BitsPerPixel / 8];
            Image.Source = bmp;
        }

        private void LinesClick(object sender, RoutedEventArgs e)
        {
            for (int y = 50; y < 250; y += 10)
            {
                for (int x = 50; x < 250; x++)
                {
                    var loc = y * bmp.BackBufferStride + x * 3;

                    pixels[loc] = 255; // Red
                    pixels[loc + 1] = 0; // Green
                    pixels[loc + 2] = 0; // Blue
                }
            }
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pixels, bmp.PixelWidth * bmp.Format.BitsPerPixel / 8, 0);
        }

        private void HorizontalClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 300; i++)
            {
                var loc = i * bmp.BackBufferStride + i * 3;

                pixels[loc] = 255; // Red
                pixels[loc + 1] = 0; // Green
                pixels[loc + 2] = 0; // Blue
            }
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pixels, bmp.PixelWidth * bmp.Format.BitsPerPixel / 8, 0);
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Title = "Please select file to save",
                InitialDirectory = Assembly.GetExecutingAssembly().Location,
                DefaultExt = ".png",
                Filter = "PNG files |*.png"
            };
            var result = sfd.ShowDialog();
            if (result == true)
            {
                // Save image here
                using (var stream = new FileStream(sfd.FileName, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    var frame = BitmapFrame.Create(bmp);

                    encoder.Frames.Add(frame);
                    encoder.Save(stream);
                }
            }
        }
    }
}
