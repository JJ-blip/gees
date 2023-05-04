namespace LsideWPF.Views
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    // converter to facilitate binding to the negative of a property
    [ValueConversion(typeof(double), typeof(string))]
    public class NegativeDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string");
            }

            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
