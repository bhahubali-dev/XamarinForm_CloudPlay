using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PlayOnCloud
{
	[ContentProperty("Type")]
	public class StyleExtension : IMarkupExtension
	{
		public string Type { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			var resolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
			if (resolver == null)
				return null;

			var type = resolver.Resolve(Type);
			return Application.Current.Resources[type.FullName] as Style;
		}
	}
}
