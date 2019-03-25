using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public partial class SearchFrame : RoundFrame
    {
        public static BindableProperty ImageSourceClearProperty =
            BindableProperty.Create("ImageSourceClear", typeof(FileImageSource), typeof(SearchFrame),
                null,
                BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (SearchFrame) bindable;
                    ctrl.ImageSourceClear = (FileImageSource) newValue;
                });

        public static BindableProperty ImageSourceProperty =
            BindableProperty.Create("ImageSource", typeof(FileImageSource), typeof(SearchFrame),
                null,
                BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (SearchFrame) bindable;
                    ctrl.ImageSource = (FileImageSource) newValue;
                });

        public static BindableProperty TextProperty =
            BindableProperty.Create("Text", typeof(string), typeof(SearchFrame),
                string.Empty,
                BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (SearchFrame) bindable;
                    ctrl.Text = (string) newValue;
                });

        public static BindableProperty TextColorProperty =
            BindableProperty.Create("TextColor", typeof(Color), typeof(SearchFrame),
                Color.Black,
                BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = (SearchFrame) bindable;
                    ctrl.TextColor = (Color) newValue;
                });

        public static BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(SearchFrame),
                null,
                BindingMode.TwoWay,
                propertyChanging: (bindable, oldValue, newValue) =>
                {
                    var ctrl = ((SearchFrame) bindable).ImageSearchButton;
                    ctrl.Command = (ICommand) newValue;
                });

        public SearchFrame()
        {
            InitializeComponent();
            //SearchPhraseEntry.DoneCommand = new Command(() => SearchPhraseEntry_OnCompleted(null, null));
        }

        [TypeConverter(typeof(ImageSourceConverter))]
        public FileImageSource ImageSource
        {
            get { return (FileImageSource) GetValue(ImageSourceProperty); }
            set
            {
                SetValue(ImageSourceProperty, value);
                ImageSearchButton.Source = value;
            }
        }

        [TypeConverter(typeof(ImageSourceConverter))]
        public FileImageSource ImageSourceClear
        {
            get { return (FileImageSource) GetValue(ImageSourceClearProperty); }
            set
            {
                SetValue(ImageSourceClearProperty, value);
                ImageClearButton.Source = value;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                SearchPhraseEntry.Text = value;
            }
        }

        public Color TextColor
        {
            get { return (Color) GetValue(TextColorProperty); }
            set
            {
                SetValue(TextColorProperty, value);
                SearchPhraseEntry.TextColor = value;
            }
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private void SearchPhraseEntry_OnCompleted(object sender, EventArgs e)
        {
            if (Command != null && Command.CanExecute(SearchPhraseEntry.Text))
                Command.Execute(SearchPhraseEntry.Text);
        }

        private void SearchPhraseEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValue(TextProperty, e.NewTextValue);
            if (SearchPhraseEntry.Text.Length > 0)
                ImageClearButton.IsVisible = true;
            else
                ImageClearButton.IsVisible = false;
        }

        private void Search_Clicked(object sender, EventArgs e)
        {
            SearchPhraseEntry.Unfocus();
        }

        private void ClearButton_OnClicked(object sender, EventArgs e)
        {
            SetValue(TextProperty, "");
            ImageClearButton.IsVisible = false;
        }
    }
}