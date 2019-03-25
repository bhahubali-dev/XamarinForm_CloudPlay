using System;
using System.Text;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class User : NotifyPropertyChanged
	{
		private string picture;
		private string firstName, lastName;
		private string email;
		private string password;
		private string authToken;
		private int credits;
		private string token;
		private long expires;

		[JsonProperty("name")]
		public string Name
		{
			get { return $"{firstName} {lastName}"; }
			set
			{
				var name = value;
				if (!string.IsNullOrEmpty(name))
				{
					var names = name.Split(' ');
					if (names.Length > 0)
						SetField(ref firstName, names[0]);

					if (names.Length > 1)
						SetField(ref lastName, names[1]);
				}
			}
		}

		public string FirstName
		{
			get { return firstName; }
			set
			{
				SetField(ref firstName, value);
				OnPropertyChanged("Name");
			}
		}

		public string LastName
		{
			get { return lastName; }
			set
			{
				SetField(ref lastName, value);
				OnPropertyChanged("Name");
			}
		}

		public string Picture
		{
			get { return picture; }
			set { SetField(ref picture, value); }
		}

		[JsonProperty("email")]
		public string Email
		{
			get { return email; }
			set { SetField(ref email, value); }
		}

		public string Password
		{
			get { return password; }
			set { SetField(ref password, value); }
		}

		public int Credits
		{
			get { return credits; }
			set
			{
				if (SetField(ref credits, value))
					OnPropertyChanged("HasCredits");
			}
		}

		[JsonIgnore]
		public bool HasCredits
		{
			get { return credits > 0; }
		}

		[JsonProperty("auth_token")]
		public string AuthToken
		{
			get { return authToken; }
			set { SetField(ref authToken, value); }
		}

		[JsonProperty("token")]
		public string Token
		{
			get { return token; }
			set { SetField(ref token, value); }
		}

		[JsonProperty("exp")]
		public long Expires
		{
			get { return expires; }
			set { SetField(ref expires, value); }
		}

		public bool IsTokenExpired()
		{
			try
			{
				if (string.IsNullOrEmpty(Token))
					return true;

				if (expires > 0)
				{
					var exp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					exp = exp.AddSeconds(expires);
					return exp < DateTime.UtcNow;
				}
			}
			catch (Exception ex)
			{
				LoggerService.Instance.Log("ERROR: User.CheckTokenExpired: " + ex);
			}

			return true;
		}
	}
}
