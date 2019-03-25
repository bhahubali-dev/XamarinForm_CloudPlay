using System;
using System.Globalization;
using Xamarin.Forms;

namespace PlayOnCloud
{
	public class NameBreadcrumbsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!string.IsNullOrWhiteSpace(value?.ToString()))
			{
				try
				{
					string breadcrumbs = value.ToString();

					if ((parameter == null) || string.IsNullOrWhiteSpace(parameter.ToString()))
					{
						ViewModel.Cloud cloud = App.Current?.BindingContext as ViewModel.Cloud;
						if (!string.IsNullOrWhiteSpace(cloud?.MediaContent?.SelectedFolder?.Name))
						{
							string selectedFolderName = (cloud.MediaContent.SelectedFolder.IsFromSearch) ?
								string.Format("Search results for \"{0}\"", cloud.MediaContent.SelectedFolder.Name) :
								cloud.MediaContent.SelectedFolder.Name;

							int currentFolderNameIndex = breadcrumbs.LastIndexOf(" > " + selectedFolderName);
							if (currentFolderNameIndex > -1)
								return breadcrumbs.Remove(currentFolderNameIndex);
						}
					}
					else if (parameter.Equals("GetOnlyLastName"))
					{
						string[] breadcrumbsElements = breadcrumbs.Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries);
						if (breadcrumbsElements?.Length > 0)
							return breadcrumbsElements[breadcrumbsElements.Length - 1];
					}
				}
				catch (Exception ex)
				{
					LoggerService.Instance.Log("ERROR: NameBreadcrumbsConverter.Convert: " + ex.Message);
				}
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
