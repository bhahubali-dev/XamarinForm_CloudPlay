using System;
using System.Runtime.Serialization;

namespace PlayOnCloud.Model
{
	[DataContract]
	[Flags]
	public enum NotificationType : int
	{
		[EnumMember]
		NewRecording = 1,
		[EnumMember]
		FailedRecording = 2,
		[EnumMember]
		RecordingIssue = 4,
		[EnumMember]
		BrowsingError = 8,
		[EnumMember]
		DownloadExpiring = 16
	}
}
