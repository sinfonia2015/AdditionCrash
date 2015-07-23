#import "FASMoviePlayController.h"
#import "AVPlayerViewController.h"

extern UIViewController *UnityGetGLViewController();

extern UIView *UnityGetGLView();

char strUrl[512];

NSURL *url;

NSString *callbackGameObjectName;

bool like;

int likeCount;

int playbackCount;

NSString *facebookUrl;

NSString *twitterText;

NSString *closeText;

NSString *userImagePath;

NSString *appIconPath;

NSString *appName;

bool userButtonEnabled;

@implementation FASMoviePlayController

static FASMoviePlayController *pInstance = nil;

+ (FASMoviePlayController*) instance {
    @synchronized(self) {
        if(pInstance == nil) {
            pInstance = [[self alloc]init];
        }
    }
    return pInstance;
}

- (void)playMovie {
    
    AVPlayerViewController *vc = [AVPlayerViewController controller:url];
        
    vc.like = like;
    
    vc.likeCount = likeCount;
    
    vc.playbackCount = playbackCount;
    
    vc.facebookUrl = facebookUrl;
    
    vc.twitterText = twitterText;
    
    vc.callbackGameObjectName = callbackGameObjectName;
    
    vc.closeText = closeText;
    
    vc.userImagePath = userImagePath;
    
    vc.appIconPath = appIconPath;
    
    vc.appName = appName;
    
    vc.userButtonEnabled = userButtonEnabled;
    
    [UnityGetGLViewController() presentViewController:vc animated:YES completion:nil];
    
    [vc autorelease];
    
    vc = nil;
}

@end

extern "C"
{
    void _FasMoviePlay(const char *url)
    {
        strncpy(strUrl, url, sizeof(strUrl));
        
        [[FASMoviePlayController instance] playMovie];
    }

    void _FasMoviePlayWithParameters(const char *_url, const char *_callbackGameObjectName, bool _like, int _likeCount, int _playbackCount, const char *_facebookUrl, const char *_twitterText, const char *_closeText, const char *_userImagePath, const char *_appIconPath, const char *_appName, bool _userButtonEnabled)
    {
        strncpy(strUrl, _url, sizeof(strUrl));
        
        NSString *str = [[NSString alloc] initWithUTF8String:strUrl];
        
        url = [NSURL URLWithString:str];
        
        callbackGameObjectName =[[NSString alloc] initWithUTF8String:_callbackGameObjectName];
        
        like = _like;
        
        likeCount = _likeCount;
        
        playbackCount = _playbackCount;
        
        facebookUrl =[[NSString alloc] initWithUTF8String:_facebookUrl];
        
        twitterText =[[NSString alloc] initWithUTF8String:_twitterText];

        closeText =[[NSString alloc] initWithUTF8String:_closeText];
        
        userImagePath =[[NSString alloc] initWithUTF8String:_userImagePath];
        
        appIconPath =[[NSString alloc] initWithUTF8String:_appIconPath];
        
        appName = [[NSString alloc] initWithUTF8String:_appName];
        
        userButtonEnabled = _userButtonEnabled;
        
        [[FASMoviePlayController instance] playMovie];
    }
}