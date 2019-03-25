using System;
using System.Collections.Generic;
using System.Text;

namespace PlayOnCloud.Model
{
	public class ProviderCode
	{
		public string Name { get; set; }

		public string Code { get; set; }

		public string AffiliateURL { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
