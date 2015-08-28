#import <UIKit/UIKit.h>
#import "AVPlayerView.h"

@interface AVPlayerViewController : UIViewController

@property NSURL* url;
@property BOOL like;
@property int likeCount;
@property int playbackCount;
@property NSString *twitterText;
@property NSString *twitterUrl;
@property NSString *facebookUrl;
@property NSString *callbackGameObjectName;
@property NSString *closeText;
@property BOOL userButtonEnabled;
@property NSString *userImagePath;
@property NSString *appIconPath;
@property NSString *appName;

@property (nonatomic, retain) IBOutlet AVPlayerView* videoPlayerView;
@property (nonatomic, retain) IBOutlet UIView*       playerToolView;
@property (nonatomic, retain) IBOutlet UIButton*     playButton;
@property (nonatomic, retain) IBOutlet UILabel*      currentTimeLabel;
@property (nonatomic, retain) IBOutlet UISlider*     seekBar;
@property (nonatomic, retain) IBOutlet UILabel*      durationLabel;

- (IBAction) pushedCloseButton:(id) sender;
- (IBAction) pushedLikeButton:(id) sender;
- (IBAction) pushedUnlikeButton:(id) sender;
- (IBAction) pushedFacebookButton:(id) sender;
- (IBAction) pushedTwitterButton:(id) sender;
- (IBAction) pushedUserButton:(id) sender;
- (IBAction) pushedAppButton:(id) sender;
- (IBAction) pushedLargePlayButton:(id) sender;

+ (AVPlayerViewController *)controller:(NSURL *)videoUrl;

@end
