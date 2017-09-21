using Foundation;
using UIKit;
using CustomerSync;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System;
using System.IO;

namespace CustomerSync.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
	{
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
            string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); 
            string libraryPath = Path.Combine (documentsPath, "..", "Library"); 
            string dbFile = Path.Combine(libraryPath, "customers.db");

			Forms.Init ();

            LoadApplication(new App(dbFile));

            return base.FinishedLaunching(app, options);
		}
	}
}

