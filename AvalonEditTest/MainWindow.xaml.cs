using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace AvalonEditTest
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Please select file to open",
                InitialDirectory = Assembly.GetExecutingAssembly().Location,
                DefaultExt = ".cs",
                Filter = "CSharp files |*.cs"
            };
            var result = ofd.ShowDialog();
            if (result == true)
            {
                var text = File.ReadAllText(ofd.FileName);
                text_editor.Text = text;
            }
        }

        private void Executeclick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(text_editor.Text);
        }
    }
}
