using System;
using Xamarin.Forms;
using CustomerSync.Models;
using System.Threading.Tasks;

namespace CustomerSync
{
	public class CustomerDetailsPage : ContentPage
	{
        Customer customer;
        Entry nameEntry;
        Entry companyEntry;
        Entry titleEntry;
        Entry emailEntry;
        Entry phoneEntry;
        Editor notesEditor;
        Button saveButton;
        Button cancelButton;
        Button deleteButton;

        public CustomerDetailsPage(Customer item)
        {
            customer = item;

            Title = String.IsNullOrWhiteSpace(item.Name) ? "Customer" : item.Name;

            // Create the controls here
            nameEntry = new Entry {
                Placeholder = "Name",
                Text = customer.Name
            };

            companyEntry = new Entry {
                Placeholder = "Company",
                Text = customer.Company
            };

            titleEntry = new Entry {
                Placeholder = "Title",
                Text = customer.Title
            };

            emailEntry = new Entry {
                Placeholder = "Email",
                Keyboard = Keyboard.Email,
                Text = customer.Email
            };

            phoneEntry = new Entry {
                Placeholder = "Phone",
                Keyboard = Keyboard.Telephone,
                Text = customer.Phone
            };

            notesEditor = new Editor {
                Text = customer.Notes,
                HeightRequest = 120
            };

            saveButton = new Button
            {
                Text = "Save"
            };
            saveButton.Clicked += async (sender, args) =>
            {
                await SaveCustomer();
                await Navigation.PopAsync();
            };

            cancelButton = new Button
            {
                Text = "Cancel"
            };
            cancelButton.Clicked += async (sender, args) => {
                await Navigation.PopAsync();
            };

            deleteButton = new Button()
            {
                Text = "Delete"
            };
            deleteButton.Clicked += async (sender, args) => {
                await DeleteCustomer();
                await Navigation.PopAsync();
            };

			Content = new ScrollView { Content = new StackLayout {
					VerticalOptions = LayoutOptions.StartAndExpand,
					Padding = new Thickness (20),
					Children = {
						// Add the controls underneath here
                        new Label { Text = "Name" }, nameEntry,
                        new Label { Text = "Company" }, companyEntry,
                        new Label { Text = "Title" }, titleEntry,
                        new Label { Text = "Email" }, emailEntry, 
                        new Label { Text = "Phone" }, phoneEntry,
                        new Label { Text = "Notes" }, notesEditor,
                        saveButton,
                        cancelButton,
                        deleteButton
					}
				}
			};
		}

		async Task SaveCustomer()
		{
			// Important to only save what has changed to reduce any
			// excess server communication
			bool hasChanged = 
				(customer.Name != nameEntry.Text) ||
				(customer.Company != companyEntry.Text) ||
				(customer.Title != titleEntry.Text) ||
				(customer.Email != emailEntry.Text) ||
				(customer.Phone != phoneEntry.Text) ||
				(customer.Notes != notesEditor.Text);

			if (hasChanged) {
				customer.Name = nameEntry.Text;
				customer.Company = companyEntry.Text;
				customer.Title = titleEntry.Text;
				customer.Email = emailEntry.Text;
				customer.Phone = phoneEntry.Text;
				customer.Notes = notesEditor.Text;

				await App.DataManager.SaveCustomer (customer);
			}
		}

        public async Task DeleteCustomer()
        {
            await App.DataManager.DeleteCustomer(customer);
        }

		protected async override void OnDisappearing ()
		{
			base.OnDisappearing ();

			await SaveCustomer ();
		}
	}
}

