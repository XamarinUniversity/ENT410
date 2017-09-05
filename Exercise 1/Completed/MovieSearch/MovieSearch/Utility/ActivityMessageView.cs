using Xamarin.Forms;

namespace MovieSearch.Utility
{
	public class ActivityMessageView : ContentView
	{
        public static readonly BindableProperty IsShowingProperty =
            BindableProperty.Create(nameof(IsShowing), typeof(bool), typeof(ActivityMessageView), false, BindingMode.OneWay);

        public bool IsShowing
        {
            get { return (bool)base.GetValue(IsShowingProperty); }
            set { base.SetValue(IsShowingProperty, value); }
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(ActivityMessageView), "Loading");

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        static void OnShowingPropertyChanging(BindableObject bindable, bool oldValue, bool newValue)
        {
            Device.BeginInvokeOnMainThread (delegate {
                ActivityMessageView mv = (ActivityMessageView) bindable;
                mv.indicator.IsRunning = newValue;
                mv.IsVisible = newValue;
            });
        }

        readonly Label messageText;
        readonly ActivityIndicator indicator;

		public ActivityMessageView ()
		{
			var layout = new StackLayout ();

            indicator = new ActivityIndicator {
				HeightRequest = 40,
			};

			messageText = new Label() { 
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Color.White
			};

            messageText.SetBinding(Label.TextProperty, 
                new Binding("Text", source: this));

			layout.Children.Add(indicator);
			layout.Children.Add(messageText);

			var frame = new Frame () {
				Content = layout,
				BackgroundColor = Color.White,
				OutlineColor = Color.Silver,
				HasShadow = false
			};

			Content = frame;
		}
	}
}
