using System;
using System.Globalization;
using Xamarin.Forms;

namespace MovieSearch
{
	public class ImageCacheConverter : IValueConverter
	{
		public int DaysToCache { get; set; } = 30;

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new UriImageSource {
				Uri = new Uri(value.ToString ()),
				CachingEnabled = true,
				CacheValidity = new TimeSpan(DaysToCache, 0, 0, 0, 0)
			};
		}

		public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException ();
		}
	}
}

