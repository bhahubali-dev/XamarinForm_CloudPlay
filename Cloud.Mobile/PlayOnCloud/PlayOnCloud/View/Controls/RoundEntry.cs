using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public class RoundEntry : Entry
    {
        public Color BorderColor { get; set; }

        public static readonly BindableProperty BorderRadiusProperty =
            BindableProperty.Create("BorderRadius", typeof(int), typeof(RoundEntry), 0);

        public static readonly BindableProperty BorderWidthProperty =
            BindableProperty.Create("BorderWidth", typeof(int), typeof(RoundEntry), 1);

        public new static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create("FontSize", typeof(double), typeof(RoundEntry), 16.0);

        public static readonly BindableProperty XAlignProperty =
            BindableProperty.Create("XAlign", typeof(TextAlignment), typeof(RoundEntry), TextAlignment.Start);

        public static readonly BindableProperty PlaceholderTextColorProperty =
            BindableProperty.Create("PlaceholderTextColor", typeof(Color), typeof(RoundEntry), Color.Default);

        public static readonly BindableProperty TagProperty =
            BindableProperty.Create("Tag", typeof(int), typeof(RoundEntry), 0);

        public static readonly BindableProperty ResponderNameProperty =
            BindableProperty.Create("ResponderName", typeof(string), typeof(RoundEntry), null);

        public static readonly BindableProperty NextResponderNameProperty =
            BindableProperty.Create("NextResponderName", typeof(string), typeof(RoundEntry), null);

        public static readonly BindableProperty AutoCapitalizeProperty =
            BindableProperty.Create("AutoCapitalize", typeof(bool), typeof(RoundEntry), false);

        public int BorderRadius
        {
            get { return (int)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public int BorderWidth
        {
            get { return (int)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public TextAlignment XAlign
        {
            get { return (TextAlignment)GetValue(XAlignProperty); }
            set { SetValue(XAlignProperty, value); }
        }

        public Color PlaceholderTextColor
        {
            get { return (Color)GetValue(PlaceholderTextColorProperty); }
            set { SetValue(PlaceholderTextColorProperty, value); }
        }

        public int Tag
        {
            get { return (int)GetValue(TagProperty); }
            set { SetValue(TagProperty, value); }
        }

        public string ResponderName
        {
            get { return (string)GetValue(ResponderNameProperty); }
            set { SetValue(ResponderNameProperty, value); }
        }

        public string NextResponderName
        {
            get { return (string)GetValue(NextResponderNameProperty); }
            set { SetValue(NextResponderNameProperty, value); }
        }

        public bool AutoCapitalize
        {
            get { return (bool)GetValue(AutoCapitalizeProperty); }
            set { SetValue(AutoCapitalizeProperty, value); }
        }
    }
}
