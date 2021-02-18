using System;
using Xamarin.Forms;

namespace XamFormsNathanDatePicker
{
    public partial class MainPage : ContentPage
    {
        private readonly StackLayout contentPage;
        private readonly NathanDatePickerView nathanDatePickerView;

        public MainPage()
        {
            Padding = 40;

            var label = new Label
            {
                Text = "date"
            };

            var button = new Button
            {
                Text = "select date"
            };

            button.Clicked += (s, e) =>
            {
                nathanDatePickerView.IsVisible = true;
            };

            contentPage = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = {label,button}
            };

            nathanDatePickerView = new NathanDatePickerView
            {
                IsVisible = false,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            nathanDatePickerView.DateSelected += (s, dateTime) =>
            {
                label.Text = $"{((DateTime)dateTime).Year}-{((DateTime)dateTime).Month}-{((DateTime)dateTime).Day}";
                nathanDatePickerView.IsVisible = false;
            };

            Content = new Grid()
            {
                Children = { contentPage, nathanDatePickerView }
            };
        }
    }
}
