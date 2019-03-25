using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class BindablePicker : Picker
	{
		public BindablePicker()
		{
			this.SelectedIndexChanged += OnSelectedIndexChanged;
		}

		public static BindableProperty ItemsSourceProperty =
		BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(BindablePicker), default(IEnumerable), propertyChanged: OnItemsSourceChanged);

		public static BindableProperty SelectedItemProperty =
			BindableProperty.Create("SelectedItem", typeof(IEnumerable), typeof(BindablePicker), default(object), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public object SelectedItem
		{
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = bindable as BindablePicker;
			picker.Items.Clear();
			if (newValue != null)
				foreach (var item in (newValue as IEnumerable))
					picker.Items.Add(item.ToString());
		}

		private void OnSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if ((SelectedIndex < 0) || (SelectedIndex > Items.Count - 1))
				SelectedItem = null;
			else
				SelectedItem = Items[SelectedIndex];
		}

		private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var picker = bindable as BindablePicker;
			if (newValue != null)
				picker.SelectedIndex = picker.Items.IndexOf(newValue.ToString());
		}
	}
}
