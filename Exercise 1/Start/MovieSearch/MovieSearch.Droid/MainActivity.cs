using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;

namespace MovieSearch.Droid
{
	[Activity (Label = "MovieSearch.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		string GetMoviesDataPath ()
		{
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal); 
			return Path.Combine (documentsPath, App.MoviesDBFilename);
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

			LoadApplication (new App (GetMoviesDataPath ()));
		}
	}
}

