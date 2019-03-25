using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace PlayOnCloud
{
    public class WrapPanel : Layout<View>
    {
        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create("Orientation", typeof(StackOrientation), typeof(WrapPanel),
                StackOrientation.Vertical,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapPanel) bindable).OnSizeChanged());

        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create("Spacing", typeof(double), typeof(WrapPanel), 6.0,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapPanel) bindable).OnSizeChanged());

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(WrapPanel), null,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapPanel) bindable).OnSizeChanged());

        public static readonly BindableProperty SelectedItemTemplateProperty =
            BindableProperty.Create("SelectedItemTemplate", typeof(DataTemplate), typeof(WrapPanel), null,
                propertyChanged: (bindable, oldvalue, newvalue) => ((WrapPanel) bindable).OnSizeChanged());

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(WrapPanel), null,
                propertyChanged: ItemsSource_OnPropertyChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create("SelectedItem", typeof(object), typeof(WrapPanel), null,
                BindingMode.OneWay,
                propertyChanged: SelectedItemChanged);

        public StackOrientation Orientation
        {
            get { return (StackOrientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public double Spacing
        {
            get { return (double) GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public DataTemplate SelectedItemTemplate
        {
            get { return (DataTemplate) GetValue(SelectedItemTemplateProperty); }
            set { SetValue(SelectedItemTemplateProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void SelectedItemChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            try
            {
                var wrapPanel = bindable as WrapPanel;
                if (wrapPanel == null || wrapPanel.SelectedItemTemplate == null || wrapPanel.ItemTemplate == null)
                    return;

                for (var i = 0; i < wrapPanel.Children.Count; i++)
                {
                    View view = wrapPanel.Children[i];
                    if (view != null)
                    {
                        var context = view.BindingContext;
                        if (context != null && (context.Equals(oldvalue) || context.Equals(newvalue)))
                        {
                            var template = wrapPanel.ItemTemplate;
                            if (context.Equals(newvalue))
                                template = wrapPanel.SelectedItemTemplate;

                            var child = template.CreateContent() as View;
                            if (child == null)
                                return;

                            child.BindingContext = context;
                            wrapPanel.Children[i] = child;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Log("ERROR: WrapPanel.SelectedItemChanged: " + ex);
            }
        }

        private static void ItemsSource_OnPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var wrapPanel = bindable as WrapPanel;
            if (wrapPanel == null)
                return;

            var value = oldvalue as INotifyCollectionChanged;
            if (value != null)
                value.CollectionChanged -= wrapPanel.OnCollectionChanged;

            value = newvalue as INotifyCollectionChanged;
            if (value != null)
            {
                if (wrapPanel.Children != null)
                    wrapPanel.Children.Clear();

                value.CollectionChanged += wrapPanel.OnCollectionChanged;

                var elements = value as IList;
                if (elements != null)
                    wrapPanel.OnCollectionChanged(bindable,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, elements));
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                Children.Clear();
                return;
            }

            if (args.OldItems != null)
                foreach (var oldItem in args.OldItems)
                    for (var i = 0; i < Children.Count; i++)
                    {
                        View view = Children[i];
                        if (view != null && view.BindingContext.Equals(oldItem))
                        {
                            Children.Remove(view);
                            view.BindingContext = null;

                            break;
                        }
                    }

            if (args.NewItems != null)
                foreach (var newItem in args.NewItems)
                {
                    var template = ItemTemplate;
                    if (newItem == SelectedItem && SelectedItemTemplate != null)
                        template = SelectedItemTemplate;

                    var child = template.CreateContent() as View;
                    if (child == null)
                        return;

                    child.BindingContext = newItem;
                    Children.Add(child);
                }
        }

        private void OnSizeChanged()
        {
            ForceLayout();
        }

        /// <summary>
        ///     This method is called during the measure pass of a layout cycle to get the desired size of an element.
        /// </summary>
        /// <param name="widthConstraint">The available width for the element to use.</param>
        /// <param name="heightConstraint">The available height for the element to use.</param>
        [Obsolete]
        protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
        {
            if (WidthRequest > 0)
                widthConstraint = Math.Min(widthConstraint, WidthRequest);

            if (HeightRequest > 0)
                heightConstraint = Math.Min(heightConstraint, HeightRequest);

            var internalWidth = double.IsPositiveInfinity(widthConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, widthConstraint);
            var internalHeight = double.IsPositiveInfinity(heightConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, heightConstraint);

            return Orientation == StackOrientation.Vertical
                ? DoVerticalMeasure(internalWidth, internalHeight)
                : DoHorizontalMeasure(internalWidth, internalHeight);
        }

        /// <summary>
        ///     Does the vertical measure.
        /// </summary>
        /// <returns>The vertical measure.</returns>
        /// <param name="widthConstraint">Width constraint.</param>
        /// <param name="heightConstraint">Height constraint.</param>
        private SizeRequest DoVerticalMeasure(double widthConstraint, double heightConstraint)
        {
            var columnCount = Device.Idiom == TargetIdiom.Phone && Device.OS == TargetPlatform.iOS ? 0 : 1;

            double width = 0;
            double height = 0;
            double minWidth = 0;
            double minHeight = 0;
            double heightUsed = 0;

            foreach (var item in Children)
            {
                var size = item.Measure(widthConstraint, heightConstraint);
                width = Math.Max(width, size.Request.Width);

                var newHeight = height + size.Request.Height + Spacing;
                if (newHeight > heightConstraint)
                {
                    columnCount++;
                    heightUsed = Math.Max(height, heightUsed);
                    height = size.Request.Height;
                }
                else
                {
                    height = newHeight;
                }

                minHeight = Math.Max(minHeight, size.Minimum.Height);
                minWidth = Math.Max(minWidth, size.Minimum.Width);
            }

            if (columnCount > 1)
            {
                height = Math.Max(height, heightUsed);
                width *= columnCount; // take max width
            }

            if (minWidth > 0)
                minWidth -= Spacing;

            if (width > 0)
                width -= Spacing;

            return new SizeRequest(new Size(width, height), new Size(minWidth, minHeight));
        }

        /// <summary>
        ///     Does the horizontal measure.
        /// </summary>
        /// <returns>The horizontal measure.</returns>
        /// <param name="widthConstraint">Width constraint.</param>
        /// <param name="heightConstraint">Height constraint.</param>
        private SizeRequest DoHorizontalMeasure(double widthConstraint, double heightConstraint)
        {
            var rowCount = 1;
            double width = 0;
            double height = 0;
            double minWidth = 0;
            double minHeight = 0;
            double widthUsed = 0;

            foreach (var item in Children)
            {
                if (item == null || !item.IsVisible)
                    continue;

                var size = item.Measure(widthConstraint, heightConstraint);
                height = Math.Max(height, size.Request.Height);

                var newWidth = width + size.Request.Width;
                if (Children.IndexOf(item) < Children.Count - 1)
                    newWidth += Spacing;

                if (newWidth > widthConstraint)
                {
                    rowCount++;
                    widthUsed = Math.Max(width, widthUsed);
                    width = 0;
                }
                else
                {
                    width = newWidth;
                }

                minHeight = Math.Max(minHeight, size.Minimum.Height);
                minWidth = Math.Max(minWidth, size.Minimum.Width);
            }

            if (rowCount > 1)
            {
                width = Math.Max(width, widthUsed);
                height = (height + Spacing) * rowCount - Spacing; // via MitchMilam 
            }

            if (minWidth > 0)
                minWidth -= Spacing;

            if (width > 0)
                width -= Spacing;

            return new SizeRequest(new Size(width, height), new Size(minWidth, minHeight));
        }

        /// <summary>
        ///     Positions and sizes the children of a Layout.
        /// </summary>
        /// <param name="x">A value representing the x coordinate of the child region bounding box.</param>
        /// <param name="y">A value representing the y coordinate of the child region bounding box.</param>
        /// <param name="width">A value representing the width of the child region bounding box.</param>
        /// <param name="height">A value representing the height of the child region bounding box.</param>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (Orientation == StackOrientation.Vertical)
            {
                double colWidth = 0;
                double yPos = y, xPos = x;

                foreach (var child in Children.Where(c => c.IsVisible))
                {
                    var request = child.Measure(width, height);

                    double childWidth = request.Request.Width;
                    double childHeight = request.Request.Height;
                    colWidth = Math.Max(colWidth, childWidth);

                    if (yPos + childHeight > height)
                    {
                        yPos = y;
                        xPos += colWidth + Spacing;
                        colWidth = 0;
                    }

                    var region = new Rectangle(xPos, yPos, childWidth, childHeight);
                    LayoutChildIntoBoundingRegion(child, region);
                    yPos += region.Height + Spacing;
                }
            }
            else
            {
                double rowHeight = 0;
                double yPos = y, xPos = x;

                foreach (var child in Children.Where(c => c.IsVisible))
                {
                    var request = child.Measure(width, height);

                    double childWidth = request.Request.Width;
                    double childHeight = request.Request.Height;
                    rowHeight = Math.Max(rowHeight, childHeight);

                    if (xPos + childWidth > width)
                    {
                        xPos = x;
                        yPos += rowHeight + Spacing;
                        rowHeight = 0;
                    }

                    var region = new Rectangle(xPos, yPos, childWidth, childHeight);
                    LayoutChildIntoBoundingRegion(child, region);
                    xPos += region.Width + Spacing;
                }
            }
        }
    }
}