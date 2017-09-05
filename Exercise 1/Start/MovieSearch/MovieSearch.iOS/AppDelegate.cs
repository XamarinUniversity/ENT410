using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using System.IO;

namespace MovieSearch.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		string GetDatabasePath()
		{
			const string sqliteFilename = "Movies.db3";

			string documentsPath = NSFileManager.DefaultManager
				.GetUrls(NSSearchPathDirectory.DocumentDirectory, 
					NSSearchPathDomain.User)[0].Path; 
			
			string storagePath = Path.Combine(documentsPath, "..", "Library", "Movies");

			if (!Directory.Exists (storagePath))
				Directory.CreateDirectory (storagePath);
			
			return Path.Combine(storagePath, sqliteFilename);
		}

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();

			LoadApplication (new App (GetDatabasePath ()));

			return base.FinishedLaunching (app, options);
		}
	}
}
