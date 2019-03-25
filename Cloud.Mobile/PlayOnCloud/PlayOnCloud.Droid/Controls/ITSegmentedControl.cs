using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

namespace PlayOnCloud.Droid.Controls
{
    public enum ITSegmentOrganizeMode : int
    {
        ITSegmentOrganizeModeHorizontal = 0,
        ITSegmentOrganizeModeVertical
    };

    //[Register("ITSegmentedControl"), DesignTimeVisible(true)]
    public class SegmentedControl: ContentView
    {
        private StackLayout layout;
        private Color tintColor = Color.Black;

        private Color unselectedColor = Color.White;
        public bool isexpand
        {
            get;
            set;
        }
        public Color TintColor
        {
            get { return tintColor; }
            set
            {
                tintColor = value;
                if (layout == null)
                {
                    return;
                }
                layout.BackgroundColor = Color.White;
                for (var iBtn = 0; iBtn < layout.Children.Count; iBtn++)
                {
                    SetSelectedState(iBtn, iBtn == selectedSegment, true);
                }
            }
        }
        private int selectedSegment;
        public int SelectedSegment
        {
            get
            {
                return selectedSegment;
            }
            set
            {
                if (value == selectedSegment)
                {
                    return;
                }
                SetSelectedState(selectedSegment, false);
                selectedSegment = value;
                if (value < 0 || value >= layout.Children.Count)
                {
                    return;
                }
                SetSelectedState(selectedSegment, true);
            }
        }
        public event EventHandler<int> SelectedSegmentChanged;
        public Command ClickedCommand;
        public SegmentedControl(bool IsExpand)
        {
            this.isexpand = IsExpand;
            layout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(0),
                Spacing = 0,
            };
            if (isexpand)
                layout.HorizontalOptions = LayoutOptions.FillAndExpand;
            else
                layout.HorizontalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.StartAndExpand;
            Padding = new Thickness(0, 0);
            Content = layout;
            selectedSegment = 0;
            ClickedCommand = new Command(SetSelectedSegment);
        }
        public void AddSegment(Xamarin.Forms.Button segment, Color othercolor)
        {
            unselectedColor = othercolor;
            tintColor = segment.BorderColor;
            segment.HorizontalOptions = LayoutOptions.FillAndExpand;
            if (isexpand)
                segment.HorizontalOptions = LayoutOptions.FillAndExpand;
            //if (App.IsTablet)
            //{
            //    segment.WidthRequest = Theme.ButtonWidth;
            //    segment.HeightRequest = Theme.ButtonHeight;
            //}

            segment.BorderRadius = 0;
            segment.BorderWidth = 1;
            segment.TextColor = TintColor;
            segment.CommandParameter = layout.Children.Count;
            segment.Command = ClickedCommand;
            layout.BackgroundColor = TintColor;
            layout.Children.Add(segment);
            SetSelectedState(layout.Children.Count - 1, layout.Children.Count - 1 == selectedSegment);
        }
        private void SetSelectedSegment(object o)
        {
            var selectedIndex = (int)o;
            SelectedSegment = selectedIndex;
            if (SelectedSegmentChanged != null)
            {
                SelectedSegmentChanged(this, selectedIndex);
            }
        }

        public void SetSegmentText(int iSegment, string segmentText)
        {
            if (iSegment >= layout.Children.Count || iSegment < 0)
            {
                throw new IndexOutOfRangeException("SetSegmentText: Attempted to change segment text for a segment doesn't exist.");
            }

            ((Xamarin.Forms.Button)layout.Children[iSegment]).Text = segmentText;
        }

        private void SetSelectedState(int indexer, bool isSelected, bool setBorderColor = true)
        {
            if (layout.Children.Count <= indexer)
            {
                return; //Out of bounds
            }
            var button = (Xamarin.Forms.Button)layout.Children[indexer];
            if (isSelected)
            {
                button.BackgroundColor = tintColor; //Color.FromHex ("#56bdbc"); //TintColor;
                button.TextColor = Color.White;
            }
            else
            {
                button.BackgroundColor = unselectedColor; //Color.White;
                button.TextColor = Color.Gray;
            }
        }
    }
}