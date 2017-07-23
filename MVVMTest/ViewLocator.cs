using System;
using System.Globalization;
using System.Windows.Data;

namespace MVVMTest
{
    public class ViewLocator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value.GetType();
            var name = type.FullName;
            var view_name = name.Replace("ViewModel", "View");
            var view_type = Type.GetType(view_name);
            return Activator.CreateInstance(view_type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
