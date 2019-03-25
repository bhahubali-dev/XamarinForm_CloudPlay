namespace PlayOnCloud.Model
{
	public class Credentials : NotifyPropertyChanged
	{
		public virtual string Username { get; set; }

		public virtual string Password { get; set; }

		public virtual string ProviderCode { get; set; }

		public virtual string ZipCode { get; set; }

		public virtual string AffiliateURL { get; set; }

		public virtual string PIN { get; set; }
	}
}