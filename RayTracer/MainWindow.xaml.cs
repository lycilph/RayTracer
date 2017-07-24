using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Engine.Geometry;
using Engine.Shapes;

namespace RayTracer
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

        private void StartClick(object sender, RoutedEventArgs e)
        {
            var sphere = new Sphere(3);
            var origin = new Point3(0, 0, -20);
            var view_width = 10;
            var view_height = 10;

            for (int y = 0; y < 300; y++)
            {
                for (int x = 0; x < 300; x++)
                {
                    var loc = y * bmp.BackBufferStride + x * 3;

                    var x_pixel = (x - 150) / 300.0 * view_width;
                    var y_pixel = (y - 150) / 300.0 * view_height;
                    var z_pixel = 0;

                    var position = new Point3(x_pixel, y_pixel, z_pixel);
                    var direction = position - origin;

                    var ray = new Ray(origin, direction);
                    var hit = sphere.Intersect(ray, out double t);

                    if (hit)
                    {
                        pixels[loc]     = 255; // Red
                        pixels[loc + 1] = 255; // Green
                        pixels[loc + 2] = 255; // Blue
                    }
                }
            }
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pixels, bmp.PixelWidth * bmp.Format.BitsPerPixel / 8, 0);
        }
    }
}
