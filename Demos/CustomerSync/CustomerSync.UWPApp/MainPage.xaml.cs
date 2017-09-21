using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CustomerSync.UWPApp
{
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var dbFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            string dbFilename = Path.Combine(dbFolder, "customers.db");
            
            LoadApplication(new CustomerSync.App(dbFilename));
        }
    }
}
