using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CustomerSyncXamForms.Cells
{
    public class CustomerCell : ViewCell
    {
        public CustomerCell() :  base()
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            layout.RowDefinitions.Add(new RowDefinition());
            layout.RowDefinitions.Add(new RowDefinition());

            switch (Device.OS)
            {
                case TargetPlatform.WinPhone:
                    layout.RowDefinitions.Add(new RowDefinition() { Height = 1 });
                    break;
            }

            layout.Padding = new Thickness(5);
            var name = new Label() { Font = Font.BoldSystemFontOfSize(14) };
            name.SetBinding(Label.TextProperty, new Binding("Name"));

            var company = new Label() { Font = Font.SystemFontOfSize(12) };
            company.SetBinding(Label.TextProperty, new Binding("Company"));

            var box = new BoxView()
            {
                WidthRequest = 30,
                HeightRequest = 30,
                Margin = new Thickness(15),
                Color = Color.LightSlateGray                
            };
            box.SetBinding(BoxView.ColorProperty, 
                new Binding("IsDeleted", converter: new Converters.StateToColorConverter()));

            layout.Children.Add(name, 0, 0);
            layout.Children.Add(company, 0, 1);

            Grid.SetColumn(box, 1);
            Grid.SetRow(box, 0);
            Grid.SetRowSpan(box, 2);
            Grid.SetColumnSpan(box, 1);

            layout.Children.Add(box);

            switch (Device.OS)
            {
                case TargetPlatform.WinPhone:
                    var line = new BoxView()
                    {
                        HeightRequest = 1,
                        Color = Color.LightGray
                    };
                    Grid.SetRow(line, 2);
                    Grid.SetColumnSpan(line, 2);

                    layout.Children.Add(line);
                    break;
            }

            View = layout;
        }
    }
}
