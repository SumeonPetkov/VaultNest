using System.Globalization;

namespace NestVault.Client.Utilities
{
    /// <summary>
    /// Maps a favorite flag to the star button color: amber when set, gray otherwise.
    /// </summary>
    public class FavoriteColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? Color.FromArgb("#F5A623") : Color.FromArgb("#919191");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
