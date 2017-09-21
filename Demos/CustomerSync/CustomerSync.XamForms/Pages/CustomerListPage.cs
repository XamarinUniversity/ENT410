using System;
using Xamarin.Forms;
using CustomerSync.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MobileSync.Models;
using CustomerSyncXamForms.Converters;
using CustomerSyncXamForms.Cells;

namespace CustomerSync
{
	public class CustomerListPage : ContentPage
	{
        readonly ObservableCollection<Customer> customers = new ObservableCollection<Customer>();

        readonly ListView listView;
        readonly ToolbarItem addButton;
        readonly ToolbarItem reloadButton;
        readonly ToolbarItem syncButton;
        readonly ToolbarItem resetDataButton;

		public CustomerListPage ()
		{
			Title = "Customers";

			var addText = Device.OnPlatform<string> ("Add", "+", "+");

			addButton = new ToolbarItem (addText, null, async () => {
				var c = new Customer ();
				c.Name = "New Customer";
				c.CreateCorrelationId ();
				customers.Add (c);

				CustomerDetailsPage details = new CustomerDetailsPage (c);
				await Navigation.PushAsync (details);
			}, ToolbarItemOrder.Primary, 0);
			ToolbarItems.Add (addButton);

            reloadButton = new ToolbarItem ("Reload", null, async () => {
                await LoadRemoteOrLocalData();
                DisplayResults();
            });
			ToolbarItems.Add (reloadButton);

			syncButton = new ToolbarItem ("Sync", null, async () => await PerformTheSync(), ToolbarItemOrder.Default, 0);
			ToolbarItems.Add (syncButton);

            resetDataButton = new ToolbarItem("Reset", null, async () =>
            {
                await App.DataManager.ResetAllData();
                await LoadRemoteOrLocalData();
                DisplayResults();
            }, ToolbarItemOrder.Default, 0);
            ToolbarItems.Add(resetDataButton);

            listView = new ListView {
                ItemsSource = customers,
                ItemTemplate = new DataTemplate(typeof(CustomerCell)),
                RowHeight = 55
			};

			Content = listView;

			DisplayProcessing ("Loading...");

            // Cannot use await in constructor, so use ContinueWith and make sure
            // we are back on UI thread.
            LoadRemoteOrLocalData()
                .ContinueWith(tr => DisplayResults(), TaskScheduler.FromCurrentSynchronizationContext());
		}

        protected override void OnAppearing ()
        {
            base.OnAppearing ();
            listView.ItemTapped += HandleItemTapped; 
        }

        protected override void OnDisappearing ()
        {
            base.OnDisappearing ();
            listView.ItemTapped -= HandleItemTapped; 
        }

		async void HandleItemTapped(object sender, ItemTappedEventArgs e)
		{
			var customer = e.Item as Customer;
			await Navigation.PushAsync(new CustomerDetailsPage(customer));
		}

		async Task<bool> LoadRemoteOrLocalData ()
		{
            this.IsBusy = true;
            try
            {
                bool RemoteDataLoaded = false;
                try
                {
                    // Useful for demonstrating that a failure will load the last copy
                    // throw new Exception("Testing failure");

                    // Load the customers from the net if available
                    var restClient = new CustomersRestClient();
                    var customersOnServer = await restClient.GetCustomers();

                    await App.DataManager.SetupDatabaseAsync();
                    await App.DataManager.UpdateCustomersAsync(customersOnServer);
                    RemoteDataLoaded = true;
                }
                catch (Exception ex)
                {
                    // We don't have the access, we need to use the local copy
                    RemoteDataLoaded = false;
                }

                // Load the local copy of the classes
                var localCustomers = await App.DataManager.GetCustomersAsync();
                customers.Clear();

                foreach (var c in localCustomers)
                    customers.Add(c);

                return RemoteDataLoaded;
            } finally
            {
                this.IsBusy = false;
            }
		}

		public async Task PerformTheSync()
		{
            this.IsBusy = true;
            try
            {

                DisplayProcessing("Synchronizing...");

                // Get the items that should be synced. Push them to the server
                DateTime syncDate = DateTime.UtcNow;

                var items = await App.DataManager.GetSyncableCustomersAsync();

                // Don't do anything if we don't need to
                if (items.Count == 0)
                {
                    DisplayResults();
                    return;
                }

                // Push the changes to the server to see what should be changed
                CustomersRestClient client = new CustomersRestClient();

                SyncResult<Customer> syncResult = null;
                bool couldSync;
                try
                {
                    syncResult = await client.SyncData(items);
                    couldSync = true;
                }
                catch (Exception ex)
                {
                    couldSync = false;
                }

                if (!couldSync)
                {
                    // We have an issue connecting to the service
                    await DisplayAlert("Could not sync", "Check your network connection and try again",
                        "OK", "Cancel");
                    return;
                }

                // Check to see if there are any conflicts and if there are present them
                switch (syncResult.Status)
                {
                    case SyncStatus.Success:
                        await App.DataManager.UpdateSyncDateTimeAsync(syncDate);
                        await App.DataManager.UpdateCorrelationIds(syncResult.CorrelationIds);
                        await App.DataManager.UpdateVersionHistory(syncResult.VersionChanges);
                        await App.DataManager.DeleteCustomers(syncResult.DeletedRecords);

                        await DisplayAlert("Sync", "The Customer Sync executed correctly", "OK");

                        break;

                    case SyncStatus.PartialSuccessWithConflict:
                        // TODO: Update the correlation Identifiers on the site
                        await App.DataManager.UpdateSyncDateTimeAsync(syncDate);
                        await App.DataManager.UpdateCorrelationIds(syncResult.CorrelationIds);
                        await App.DataManager.UpdateVersionHistory(syncResult.VersionChanges);
                        await App.DataManager.DeleteCustomers(syncResult.DeletedRecords);

                        var details = await DisplayAlert("Sync Conflict", "You have sync conflicts. Do you want to force the changes?", "OK", "Cancel");
                        if (details)
                        {
                            try
                            {
                                await client.SyncData(items, true);
                                await DisplayAlert("Sync Conflict", "The Sync has been applied", "Close");
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Sync Conflict", "Could not process: " + ex.Message, "Close");
                            }
                        }
                        break;
                    case SyncStatus.Failed:
                        await DisplayAlert("Sync", "The Customer Sync failed", "OK");
                        break;
                }

                await LoadRemoteOrLocalData();
                DisplayResults();
            } finally
            {
                this.IsBusy = false;
            }
        }

		void DisplayProcessing(string message)
		{
			listView.ItemTemplate = new DataTemplate (typeof(TextCell));
			listView.ItemsSource = new [] { message };
		}

		void DisplayResults()
		{
			listView.ItemsSource = customers;
            listView.ItemTemplate = new DataTemplate(typeof(CustomerCell));
		}
	}

	public class LoadingResultsCell : ViewCell
	{
		public LoadingResultsCell ()
		{
			ActivityIndicator indicator = new ActivityIndicator ();

			var loadingMessage = new Label () {
				Text = "Loading Results..."
			};
			loadingMessage.SetBinding (Label.TextProperty, new Binding ("."));

			var stackInfo = new StackLayout () {
				Padding = new Thickness (20),
				Children = {
					indicator,
					loadingMessage        
				}
			};

			View = stackInfo;
		}
	}
}