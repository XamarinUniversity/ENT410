using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]

namespace MovieSearch
{
	public class App : Application
	{
		public static string MoviesDBFilename = "Movies.db3";

		public App(string dbFile)
		{
			MainPage = new ContentPage
			{
				BackgroundColor = Color.FromRgb(58, 153, 216),
				Content = new ActivityIndicator
				{
					Color = Color.White,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					IsRunning = true,
				}
			};

			DataManager.SetupDatabaseAsync(dbFile)
				.ContinueWith(tr => {
					MainPage = new NavigationPage(new MovieSearchPage());
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}

