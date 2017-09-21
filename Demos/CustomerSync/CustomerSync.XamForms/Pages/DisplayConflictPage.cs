using System;
using CustomerSync.Models;
using MobileSync.Models;
using Xamarin.Forms;
using System.Collections.Generic;

namespace CustomerSync
{
	public class CustomerConflictInfoCell : ViewCell
	{
		public CustomerConflictInfoCell ()
		{
			List<View> allItems = new List<View> ();

			// Links against a conflict item
			var conflictSummary = new Label {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
			};
			conflictSummary.SetBinding(Label.TextProperty, new Binding("ConflictMessage"));
			allItems.Add (conflictSummary);

			// Add the details for the other columns
            string[] properties = {	"Name", "Company", "Title", "Email", "Phone", "Notes" };

			foreach (var property in properties) 
            {
				var columnTitle = new Label { Text = property + " Changes" };
				allItems.Add (columnTitle);

				var serverVersion = new Label();
				serverVersion.SetBinding(Label.TextProperty, new Binding("CurrentItem." + property));
				allItems.Add (serverVersion);

				var localVersion = new Label();
				localVersion.SetBinding (Label.TextProperty, new Binding ("RequestedUpdateItem." + property));
				allItems.Add (localVersion);
			}

			// We're going to display the differences in the cell: It will be of the format:
			var layout = new StackLayout {
				Padding = new Thickness (20, 0, 0, 0),
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.StartAndExpand,
			};

			foreach (var item in allItems)
				layout.Children.Add(item);

			View = layout;
		}
	}

	public class ConflictDetailsPage : ContentPage
	{
		public ConflictDetailsPage (SyncResult<Customer> items)
		{
			Title = "Tap to overwrite";

			var listView = new ListView {
				ItemsSource = items.Conflicts,
				ItemTemplate = new DataTemplate (typeof(CustomerConflictInfoCell))
			};

			listView.ItemSelected += async (s, e) => {
				var item = e.SelectedItem as ConflictItem<Customer>;

				List<Customer> customers = new List<Customer>();
				customers.Add(item.RequestedUpdateItem);

				// Force the change to the server and then remove the item
				CustomersRestClient client = new CustomersRestClient();
				var response = await client.SyncData(customers, true);

			};

			Content = listView;
		}
	}
}

