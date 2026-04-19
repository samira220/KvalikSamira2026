using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KvalikSamira.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        private static readonly string AssemblyName =
            typeof(ImagePathConverter).Assembly.GetName().Name ?? "KvalikSamira";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string imagePath || string.IsNullOrWhiteSpace(imagePath))
                return null;

            string[] folders;
            if (imagePath.StartsWith("Pr", StringComparison.OrdinalIgnoreCase))
                folders = new[] { "кастом", "Кастом" };
            else if (imagePath.StartsWith("KL", StringComparison.OrdinalIgnoreCase))
                folders = new[] { "косплей", "Косплей" };
            else
                return null;

            foreach (var folder in folders)
            {
                var uri = new Uri($"avares://{AssemblyName}/Ресурсы/pr/{folder}/{imagePath}");
                try
                {
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }
                catch
                {
                }
            }

            foreach (var folder in folders)
            {
                var uri = new Uri($"avares://{AssemblyName}/Assets/{folder}/{imagePath}");
                try
                {
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }
                catch
                {
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
