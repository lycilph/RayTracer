using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Engine.Geometry;
using Engine.Shapes;
using NLog;
using ReactiveUI;

namespace RayTracer
{
    public partial class MainWindow
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private DispatcherTimer timer;
        private Stopwatch stopwatch;
        private WriteableBitmap bmp;
        private byte[] pixels;

        public ReactiveCommand ExitCommand { get; set; }
        
        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public string TimeText
        {
            get { return (string)GetValue(TimeTextProperty); }
            set { SetValue(TimeTextProperty, value); }
        }
        public static readonly DependencyProperty TimeTextProperty =
            DependencyProperty.Register("TimeText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public BitmapSource Image
        {
            get { return (BitmapSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(BitmapSource), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            bmp = new WriteableBitmap(300, 300, 96, 96, PixelFormats.Rgb24, null);
            pixels = new byte[bmp.PixelHeight * bmp.PixelWidth * bmp.Format.BitsPerPixel / 8];
            Image = bmp;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += UpdateTimer;

            stopwatch = new Stopwatch();

            ExitCommand = ReactiveCommand.Create(() => Close());
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            TimeText = $"Elapsed {stopwatch.Elapsed.TotalSeconds:0.00} sec";
        }

        private void ParseClick(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                stopwatch.Stop();
                stopwatch.Reset();
                timer.Stop();
            }
            else
            {
                stopwatch.Start();
                timer.Start();
            }
        }

        private void RunClick(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();

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
                        pixels[loc] = 255; // Red
                        pixels[loc + 1] = 255; // Green
                        pixels[loc + 2] = 255; // Blue
                    }
                }
            }
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), pixels, bmp.PixelWidth * bmp.Format.BitsPerPixel / 8, 0);

            sw.Stop();
            StatusText = $"Rendering took {sw.ElapsedMilliseconds} ms";
        }
    }
}
