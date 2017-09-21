using Xamarin.Forms;

namespace CustomerSync
{
	public class App : Application
	{
        public static DataManager DataManager { get; private set; }

        public App(string filename)
		{	
            DataManager = new DataManager(filename);
            MainPage = new NavigationPage (new CustomerListPage());
		}
	}
}