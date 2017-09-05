using Xamarin.Forms;

namespace MovieSearch.Utility
{
	public class FramedLabel : ContentView
	{
		readonly Label messageText;

		public string Text {
			set {
				messageText.Text = value;
			}
		}

		public FramedLabel() : base()
		{
			messageText = new Label () { 
				Text = "Loading",
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
				TextColor = Color.Gray,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
			};

			var frame = new Frame () {
				Content = messageText,
				Padding = new Thickness (10),
				BackgroundColor = Color.White,
				OutlineColor = Color.Silver,
				HasShadow = false
			};

			Content = frame;
		}
	}
}
