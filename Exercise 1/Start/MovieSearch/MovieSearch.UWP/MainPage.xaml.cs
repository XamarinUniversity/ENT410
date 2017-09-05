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

namespace MovieSearch.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var path = GetMoviesDataPath();

            LoadApplication(new MovieSearch.App(GetMoviesDataPath()));
        }

        string GetMoviesDataPath()
        {
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

            return System.IO.Path.Combine(path, MovieSearch.App.MoviesDBFilename);
        }
    }
}
