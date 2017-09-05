using Xamarin.Forms;

namespace MovieSearch
{
	public partial class MovieDetailsPage : ContentPage
	{
		public MovieDetailsPage (Movie movie)
		{
			this.BindingContext = movie;

			InitializeComponent ();
		}
	}
}

