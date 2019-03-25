using Foundation;
using ObjCRuntime;
using UIKit;

namespace FBNotificationsBindings
{
	// @protocol FBNCardViewControllerDelegate <NSObject>
	[BaseType(typeof(NSObject))]
	[Model]
	interface FBNCardViewControllerDelegate
	{
		// @optional -(void)pushCardViewController:(FBNCardViewController * _Nonnull)controller willDismissWithOpenURL:(NSURL * _Nonnull)url;
		[Export("pushCardViewController:willDismissWithOpenURL:")]
		void PushCardViewController(FBNCardViewController controller, NSUrl url);

		// @optional -(void)pushCardViewControllerWillDismiss:(FBNCardViewController * _Nonnull)controller;
		[Export("pushCardViewControllerWillDismiss:")]
		void PushCardViewControllerWillDismiss(FBNCardViewController controller);
	}

	// @interface FBNCardViewController : UIViewController
	[BaseType(typeof(UIViewController))]
	[DisableDefaultCtor]
	interface FBNCardViewController
	{
		[Wrap("WeakDelegate")]
		[NullAllowed]
		FBNCardViewControllerDelegate Delegate { get; set; }

		// @property (nonatomic, weak) id<FBNCardViewControllerDelegate> _Nullable delegate;
		[NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
		NSObject WeakDelegate { get; set; }
	}

	[Static]
	partial interface Constants
	{
		// extern NSString * _Nonnull FBNotificationsErrorDomain;
		[Field("FBNotificationsErrorDomain", "__Internal")]
		NSString FBNotificationsErrorDomain { get; }

		// extern NSString * _Nonnull FBNotificationsCardFormatVersionString;
		[Field("FBNotificationsCardFormatVersionString", "__Internal")]
		NSString FBNotificationsCardFormatVersionString { get; }

		// extern double FBNotificationsVersionNumber;
		[Field("FBNotificationsVersionNumber", "__Internal")]
		double FBNotificationsVersionNumber { get; }

		// extern const unsigned char [] FBNotificationsVersionString;
		[Field("FBNotificationsVersionString", "__Internal")]
		System.IntPtr FBNotificationsVersionString { get; }
	}

	// typedef void (^FBNCardContentPreparationCompletion)(NSDictionary * _Nullable, NSError * _Nullable);
	delegate void FBNCardContentPreparationCompletion([NullAllowed] NSDictionary arg0, [NullAllowed] NSError arg1);

	// typedef void (^FBNCardPresentationCompletion)(FBNCardViewController * _Nullable, NSError * _Nullable);
	delegate void FBNCardPresentationCompletion([NullAllowed] FBNCardViewController arg0, [NullAllowed] NSError arg1);

	// typedef void (^FBNLocalNotificationCreationCompletion)(UILocalNotification * _Nullable, NSError * _Nullable);
	delegate void FBNLocalNotificationCreationCompletion([NullAllowed] UILocalNotification arg0, [NullAllowed] NSError arg1);

	// @interface FBNotificationsManager : NSObject
	[BaseType(typeof(NSObject))]
	[DisableDefaultCtor]
	interface FBNotificationsManager
	{
		// +(instancetype _Nonnull)sharedManager;
		[Static]
		[Export("sharedManager")]
		FBNotificationsManager SharedManager();

		// -(void)preparePushCardContentForRemoteNotificationPayload:(NSDictionary * _Nonnull)payload completion:(FBNCardContentPreparationCompletion _Nullable)completion;
		[Export("preparePushCardContentForRemoteNotificationPayload:completion:")]
		void PreparePushCardContentForRemoteNotificationPayload(NSDictionary payload, [NullAllowed] FBNCardContentPreparationCompletion completion);

		// -(void)presentPushCardForRemoteNotificationPayload:(NSDictionary * _Nonnull)payload fromViewController:(UIViewController * _Nullable)viewController completion:(FBNCardPresentationCompletion _Nullable)completion;
		[Export("presentPushCardForRemoteNotificationPayload:fromViewController:completion:")]
		void PresentPushCardForRemoteNotificationPayload(NSDictionary payload, [NullAllowed] UIViewController viewController, [NullAllowed] FBNCardPresentationCompletion completion);

		// -(BOOL)canPresentPushCardFromRemoteNotificationPayload:(NSDictionary * _Nullable)payload;
		[Export("canPresentPushCardFromRemoteNotificationPayload:")]
		bool CanPresentPushCardFromRemoteNotificationPayload([NullAllowed] NSDictionary payload);

		// -(void)createLocalNotificationFromRemoteNotificationPayload:(NSDictionary * _Nonnull)payload completion:(FBNLocalNotificationCreationCompletion _Nonnull)completion;
		[Export("createLocalNotificationFromRemoteNotificationPayload:completion:")]
		void CreateLocalNotificationFromRemoteNotificationPayload(NSDictionary payload, FBNLocalNotificationCreationCompletion completion);

		// -(void)presentPushCardForLocalNotification:(UILocalNotification * _Nonnull)notification fromViewController:(UIViewController * _Nullable)viewController completion:(FBNCardPresentationCompletion _Nullable)completion;
		[Export("presentPushCardForLocalNotification:fromViewController:completion:")]
		void PresentPushCardForLocalNotification(UILocalNotification notification, [NullAllowed] UIViewController viewController, [NullAllowed] FBNCardPresentationCompletion completion);

		// -(BOOL)canPresentPushCardFromLocalNotification:(UILocalNotification * _Nonnull)notification;
		[Export("canPresentPushCardFromLocalNotification:")]
		bool CanPresentPushCardFromLocalNotification(UILocalNotification notification);
	}
}