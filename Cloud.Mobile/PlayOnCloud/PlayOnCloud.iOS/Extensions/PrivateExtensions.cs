using System;
using System.Reflection;

namespace PlayOnCloud.iOS.Extensions
{
	public static class PrivateExtensions
	{
		public static T GetFieldValue<T>(this object @this, Type type, string name)
		{
			var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
			return (T)field.GetValue(@this);
		}
	}
}
