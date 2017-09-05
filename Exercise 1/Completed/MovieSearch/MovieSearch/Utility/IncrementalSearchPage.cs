using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace MovieSearch.Utility
{
	public abstract class IncrementalSearchPage<T> : ContentPage where T : class
	{
        public FramedLabel OfflineIndicator { get; private set; }
        public SearchBar SearchBar { get; private set; }
        public ListView ListView { get; private set; }
        public ActivityMessageView ActivityView { get; private set; }

        public ObservableCollection<T> AllItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int RowsBeforeTheEndToLoad { get; set; }
        public string LastSearch { get; protected set; }
        public bool HasMoreData { get; protected set; }
        public bool IsLoading { get; protected set; }
        public DataLoadLocation DataLocation { get; protected set; }

		protected IncrementalSearchPage ()
		{
            RowsBeforeTheEndToLoad = 10;
            LastSearch = String.Empty;
            DataLocation = DataLoadLocation.None;
            AllItems = new ObservableCollection<T>();

			SearchBar = new SearchBar { Placeholder = "Search for..." };
			SearchBar.SearchButtonPressed += async (sender, args) => await LoadItemsAsync (SearchBar.Text);

			ActivityView = new ActivityMessageView {
				IsVisible = false
			};

            ActivityView.SetBinding(ActivityMessageView.IsShowingProperty, 
                new Binding("IsBusy", source: this));

			OfflineIndicator = new FramedLabel {
				IsVisible = false,
                Text = "Currently offline - using cached data"
			};

			this.ListView = new ListView (ListViewCachingStrategy.RecycleElement) {
				ItemsSource = AllItems,
				ItemTemplate = new DataTemplate (typeof(TextCell))
			};

            this.ListView.ItemTapped += async (s, e) => {
				var item = e.Item as T;
                this.ListView.SelectedItem = null;
				await ItemSelectedAsync(item);
			};

			// Provide Access for infinite scrolling by loading more data
            // when you are at the last record
            this.ListView.ItemAppearing += async (sender, e) => {

                if (HasMoreData && !IsLoading)
                {
                    var foundIndex = AllItems.IndexOf(e.Item as T);
                    if (foundIndex == AllItems.Count - RowsBeforeTheEndToLoad)
                        await LoadNextPageAsync();
                }
            };

			this.ListView.IsPullToRefreshEnabled = true;
			this.ListView.Refreshing += async (sender, e) => {
				await LoadItemsAsync (LastSearch);
				this.ListView.IsRefreshing = false;
			};

			var layout = new RelativeLayout ();

			layout.Children.Add (
				SearchBar, 
				Constraint.Constant (0), 
				Constraint.Constant (0), 
				Constraint.RelativeToParent (p => p.Width),
				Constraint.Constant (50)
			);

			layout.Children.Add (
                this.ListView, 
				Constraint.Constant (0), 
				Constraint.Constant (50),
				Constraint.RelativeToParent (p => p.Width),
				Constraint.RelativeToParent (p => p.Height - 50)
			);

			layout.Children.Add (
				OfflineIndicator, 
				Constraint.Constant (0),
				Constraint.RelativeToParent (p => p.Height - 45),
				Constraint.RelativeToParent (p => p.Width),
				Constraint.Constant (45)
			);

			layout.Children.Add (
                ActivityView, 
				Constraint.RelativeToParent (p => p.Width - 100),
				Constraint.RelativeToParent (p => p.Height - 100),
				Constraint.Constant (90),
				Constraint.Constant (90)
			);

			Content = layout;
		}


		public Task LoadItemsAsync (string text)
		{
			LastSearch = text;
			CurrentPage = 0;
			HasMoreData = true;
			AllItems.Clear ();
			IsLoading = true;

			return DoLoadAsync ();
		}

        public Task LoadFirstPageAsync ()
        {
            LastSearch = string.Empty;
            CurrentPage = 0;
            HasMoreData = true;
            AllItems.Clear ();
            IsLoading = true;

            return DoLoadAsync ();
        }

		public Task LoadNextPageAsync ()
		{
			CurrentPage++;
			return DoLoadAsync ();
		}

		private async Task DoLoadAsync ()
		{
            IsBusy = true;

			bool couldLoad = false;

			try
			{
				// Need to define the action to call
				var data = await LoadPageFromNetworkAsync ();

				foreach (T item in data)
					AllItems.Add (item);

				couldLoad = true;
				DataLocation = DataLoadLocation.RemoteService;

			} 
            catch (Exception e) 
            {
				HandleLoadException (e);
				couldLoad = false;
			}

			if (!couldLoad) 
            {
				DataLocation = DataLoadLocation.Cache;

				try
				{
					AllItems.Clear();

					var data = await LoadDataFromCacheAsync ();

					foreach (T item in data)
						AllItems.Add (item);

					HasMoreData = false;

				} 
                catch (Exception e) 
                {
					HandleLoadException (e);
				}

				DataLocation = DataLoadLocation.Cache;
			}

			UpdateOfflineIndicator ();

            IsBusy = false;
			IsLoading = false;
		}

		void UpdateOfflineIndicator()
		{
            OfflineIndicator.IsVisible = (DataLocation == DataLoadLocation.Cache);
		}

        protected virtual void HandleLoadException(Exception e)
        {
            Debug.WriteLine(e.ToString());
        }

		protected abstract Task ItemSelectedAsync (T item);
		protected abstract Task<IList<T>> LoadPageFromNetworkAsync ();
		protected abstract Task<IList<T>> LoadDataFromCacheAsync ();
	}
}
