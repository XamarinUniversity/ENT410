using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using MovieSearch.Utility;

namespace MovieSearch
{
	public class MovieSearchPage : IncrementalSearchPage<Movie>
	{
		public MovieSearchPage()
		{
			Title = "Movies";
			ListView.ItemTemplate = new DataTemplate (typeof(ImageCell)) {
				Bindings = {
					{ ImageCell.ImageSourceProperty, new Binding("ArtworkUri", converter: new ImageCacheConverter()) },
					{ TextCell.TextProperty, new Binding ("Title") },
					{ TextCell.DetailProperty, new Binding ("Genre") }
				}
			};

            LoadFirstPageAsync().IgnoreResult();
		}

		protected async override Task ItemSelectedAsync (Movie item)
		{
			var page = new MovieDetailsPage (item);
			await Navigation.PushAsync (page);
		}

		protected async override Task<IList<Movie>> LoadPageFromNetworkAsync ()
		{
			var service = new MovieRestService ();

			var data = await service.GetMoviesForSearchAsync(LastSearch, CurrentPage);
			HasMoreData = data.Count == service.NumberOfMoviesPerRequest;

			return data;
		}

		protected override async Task<IList<Movie>> LoadDataFromCacheAsync ()
		{
			return null;
        }
	}
} 