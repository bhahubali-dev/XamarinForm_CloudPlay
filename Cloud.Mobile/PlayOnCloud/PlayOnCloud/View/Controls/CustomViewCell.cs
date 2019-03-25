using Xamarin.Forms;

namespace PlayOnCloud
{
    public class CustomViewCell : ViewCell
    {
        public static readonly BindableProperty SelectedBackgroundColorProperty =
            BindableProperty.Create("SelectedBackgroundColor", typeof(Color), typeof(CustomViewCell), Color.Black);

        public static readonly BindableProperty CanMoveProperty =
            BindableProperty.Create("CanEditRow", typeof(bool), typeof(CustomListView), false);

        public static readonly BindableProperty CheckedProperty =
            BindableProperty.Create("Checked", typeof(bool), typeof(CustomListView), false, BindingMode.TwoWay);


        /// <summary>
        ///     Gets or sets the SelectedBackgroundColor.
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return (Color) GetValue(SelectedBackgroundColorProperty); }
            set { SetValue(SelectedBackgroundColorProperty, value); }
        }


        public bool CanMove
        {
            get { return (bool) GetValue(CanMoveProperty); }
            set { SetValue(CanMoveProperty, value); }
        }

        public bool Checked
        {
            get { return (bool) GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }
    }
}