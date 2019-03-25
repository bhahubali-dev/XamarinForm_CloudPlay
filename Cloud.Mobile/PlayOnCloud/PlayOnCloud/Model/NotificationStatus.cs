using System.Runtime.Serialization;

namespace PlayOnCloud.Model
{
	[DataContract]
	public enum NotificationStatus : int
	{
		[EnumMember]
		Unread = 0,
		[EnumMember]
		Read = 1,
		[EnumMember]
		Deleted = 2
	}
}
