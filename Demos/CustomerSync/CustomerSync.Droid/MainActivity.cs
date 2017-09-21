using Android.App;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using CustomerSync;
using Android.Content.PM;

namespace CustomerSync.Droid
{
	[Activity (Label = "Customer Sync", MainLauncher = true, 
        Theme = "@android:style/Theme.Holo.Light", Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments); 
            string dbFile = System.IO.Path.Combine(documentsPath, "customers.db");

			Forms.Init (this, bundle);
            LoadApplication(new App(dbFile));
		}
	}
}


