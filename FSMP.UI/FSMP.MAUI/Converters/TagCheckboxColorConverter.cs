using System.Globalization;

namespace FSMP.MAUI.Converters;

public class TagCheckboxColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not bool isSaved || values[1] is not bool isPending)
            return Colors.Transparent;

        if (isPending)
            return Color.FromArgb("#FF9800"); // Orange

        if (isSaved)
            return Color.FromArgb("#4CAF50"); // Green

        return Colors.LightGray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
