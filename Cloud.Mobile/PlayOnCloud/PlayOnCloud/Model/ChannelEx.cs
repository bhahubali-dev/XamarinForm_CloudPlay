using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace PlayOnCloud.Model
{
	public class ChannelEx : Channel, IContentItem
	{
		private ChannelLoginInfo loginInfo;
		private ObservableCollection<IContentItem> children;
		private bool expired;

		public ChannelEx()
		{
			loginInfo = new ChannelLoginInfo();
		}

		public ChannelLoginInfo LoginInfo
		{
			get { return loginInfo; }
			set { SetField(ref loginInfo, value); }
		}

		public bool IsChannel
		{
			get { return true; }
			set { }
		}

		public bool IsRoot
		{
			get { return false; }
		}

		public bool IsFolder
		{
			get { return true; }
		}

		[JsonIgnore]
		public bool Expired
		{
			get { return expired; }
			set { SetField(ref expired, value); }
		}

		public ObservableCollection<IContentItem> Children
		{
			get { return children; }
			set
			{
				if (SetField(ref children, value) && (children != null))
					foreach (var child in children)
						child.Parent = this;
			}
		}

		public IContentItem Parent { get; set; }

		[JsonIgnore]
		public string SmallThumbnailUrl
		{
			get { return ContentClient.GetSmallImageUrl(ID); }
		}

		[JsonIgnore]
		public string LargeThumbnailUrl
		{
			get { return ContentClient.GetLargeImageUrl(ID); }
		}

		[JsonIgnore]
		public string MastheadUrl
		{
			get
			{
				if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Phone)
					return ContentClient.GetSmallMastheadUrl(ID);

				return ContentClient.GetMediumMastheadUrl(ID);
			}
		}

		[JsonIgnore]
		public string LoginImageUrl
		{
			get { return ContentClient.GetLoginImageUrl(ID); }
		}

		[JsonIgnore]
		public bool IsSearchVisible
		{
			get
			{
				if (IsSearchable)
				{
					if ((CredentialsType == ChannelCredentialsType.UsernamePassword) ||
						(CredentialsType == ChannelCredentialsType.UsernamePasswordPin) ||
						(CredentialsType == ChannelCredentialsType.UsernamePasswordServiceProvider))
						return (LoginInfo != null) && LoginInfo.HasCredentials && LoginInfo.LoginPerformed;

					return true;
				}

				return false;
			}
		}

		[JsonIgnore]
		public bool IsFromSearch
		{
			get { return false; }
			set { }
		}

		[JsonIgnore]
		public bool IsDeepLink
		{
			get { return false; }
			set { }
		}

		public void RefreshIsSearchVisible()
		{
			OnPropertyChanged("IsSearchVisible");
		}
	}

	public class ChannelLoginInfo : CredentialsEx
	{
		private ObservableCollection<ProviderCode> cableProviders;
		private bool loginPerformed;
		private bool validationSuccessful;
		private bool hideLoginInfoBar;

		[JsonIgnore]
		public ObservableCollection<ProviderCode> CableProviders
		{
			get { return cableProviders; }
			set { SetField(ref cableProviders, value); }
		}

		[JsonIgnore]
		public ProviderCode SelectedCableProvider
		{
			get
			{
				if (!string.IsNullOrEmpty(ProviderCode) && (cableProviders != null))
					return cableProviders.FirstOrDefault(c => c.Code == ProviderCode);

				return null;
			}
		}

		[JsonIgnore]
		public int SelectedCableProviderIndex
		{
			get
			{
				if (!string.IsNullOrEmpty(ProviderCode) && (cableProviders != null))
				{
					var selected = cableProviders.FirstOrDefault(c => c.Code == ProviderCode);
					if (selected != null)
						return cableProviders.IndexOf(selected);
				}

				return -1;
			}
			set
			{
				if ((cableProviders != null) && (value >= 0) && (value < cableProviders.Count))
				{
					ProviderCode = cableProviders[value].Code;
					AffiliateURL = cableProviders[value].AffiliateURL;
				}

				OnPropertyChanged("SelectedCableProviderIndex");
			}
		}

		[JsonIgnore]
		public bool LoginPerformed
		{
			get { return loginPerformed; }
			set { SetField(ref loginPerformed, value); }
		}

		public bool ValidationSuccessful
		{
			get { return validationSuccessful; }
			set { SetField(ref validationSuccessful, value); }
		}

		[JsonIgnore]
		public bool HideLoginInfoBar
		{
			get { return hideLoginInfoBar; }
			set { SetField(ref hideLoginInfoBar, value); }
		}

		public void Clear()
		{
			Username = null;
			Password = null;
			ZipCode = null;
			PIN = null;
		}
	}
}
