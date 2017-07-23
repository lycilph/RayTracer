using System.Windows;

namespace MVVMTest
{
    public partial class MainWindow
    {
        public IPage HeaderPage
        {
            get { return (IPage)GetValue(HeaderPageProperty); }
            set { SetValue(HeaderPageProperty, value); }
        }
        public static readonly DependencyProperty HeaderPageProperty =
            DependencyProperty.Register("HeaderPage", typeof(IPage), typeof(MainWindow), new PropertyMetadata(null));

        public IPage MainPage
        {
            get { return (IPage)GetValue(MainPageProperty); }
            set { SetValue(MainPageProperty, value); }
        }
        public static readonly DependencyProperty MainPageProperty =
            DependencyProperty.Register("MainPage", typeof(IPage), typeof(MainWindow), new PropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            HeaderPage = new HeaderPageViewModel();
            MainPage = new MainPageViewModel();
        }
    }
}
