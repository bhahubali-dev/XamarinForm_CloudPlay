using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class CredentialsEx : Credentials
	{
		public override string Username
		{
			get { return base.Username; }
			set
			{
				base.Username = value;
				OnPropertyChanged("HasCredentials");
			}
		}

		public override string Password
		{
			get { return base.Password; }
			set
			{
				base.Password = value;
				OnPropertyChanged("HasCredentials");
			}
		}

		public override string ZipCode
		{
			get { return base.ZipCode; }
			set
			{
				base.ZipCode = value;
				OnPropertyChanged("HasCredentials");
			}
		}

		[JsonIgnore]
		public bool HasCredentials
		{
			get { return (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)) || !string.IsNullOrEmpty(ZipCode); }
		}
	}
}
