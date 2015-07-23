#import "FASUtil.h"

extern "C"{
    
    int isDevelopment() {
        
#if TARGET_IPHONE_SIMULATOR
        return 1;
#else
        static BOOL isDevelopment = NO;
        
        NSData *data = [NSData dataWithContentsOfFile:[NSBundle.mainBundle pathForResource:@"embedded" ofType:@"mobileprovision"]];
        
        if (data)
        {
            NSUInteger len = [data length];
            
            char *bytes = (char*)malloc(len);
            
            //const char *bytes = [data bytes];
            
            memcpy(bytes, [data bytes], len);
            
            NSMutableString *profile = [[NSMutableString alloc] initWithCapacity:data.length];
            
            for (NSUInteger i = 0; i < data.length; i++)
            {
                [profile appendFormat:@"%c", bytes[i]];
            }
            
            // Look for debug value, if detected we're a development build.
            NSString *cleared = [[profile componentsSeparatedByCharactersInSet:NSCharacterSet.whitespaceAndNewlineCharacterSet] componentsJoinedByString:@""];
            
            isDevelopment = [cleared rangeOfString:@"<key>get-task-allow</key><true/>"].length > 0;
            
            free(bytes);
        }
        
        return (isDevelopment) ? 1 : 0;
#endif
        
    }
    
    void attemptRotationToDeviceOrientation() {
        
        [UIViewController attemptRotationToDeviceOrientation];
    }
    
    const char* _FasGetVideoThumbnailImagePath(const char* videofilepath){
        
        NSString *filepath = [NSString stringWithCString: videofilepath encoding:NSUTF8StringEncoding];
        
        NSURL *url = [NSURL fileURLWithPath:filepath];
        
        AVURLAsset *asset = [[AVURLAsset alloc] initWithURL:url options:nil];
        
        AVAssetImageGenerator *generate = [[AVAssetImageGenerator alloc] initWithAsset:asset];
        
        NSError *err = NULL;
        
        CMTime time = CMTimeMake(1, 60);
        
        CGImageRef imgRef = [generate copyCGImageAtTime:time actualTime:NULL error:&err];
        
        NSLog(@"err==%@, imageRef==%@", err, imgRef);
        
        UIImage *thumbnail =  [[UIImage alloc] initWithCGImage:imgRef];
        
        NSData *imageData = UIImagePNGRepresentation(thumbnail);
        
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        
        NSString *documentsDirectory = [paths objectAtIndex:0];
        
        NSString *filePath = [documentsDirectory stringByAppendingPathComponent:@"TmpThumbnail.png"];
        
        //file name
        NSLog(@"filePath %@",filePath);
        [imageData writeToFile:filePath atomically:YES];
        
        const char *str = [filePath UTF8String];
        assert(str);
        char *copyStr = (char *)malloc(strlen(str)+1);
        assert(copyStr);
        strcpy(copyStr, str);
        
        return copyStr;
    }
    
    UIActivityIndicatorView *fasAi ;
    
    void _FasStartAcitvityIndicator(bool operatable){
        
        if(fasAi != NULL)
            [fasAi stopAnimating];
        
        fasAi = NULL;
        
        UIViewController* parent = UnityGetGLViewController();
        
        fasAi = [[UIActivityIndicatorView alloc] init];
        fasAi.frame = parent.view.bounds;
        fasAi.center = parent.view.center;
        fasAi.activityIndicatorViewStyle = UIActivityIndicatorViewStyleWhite;
        
        fasAi.layer.backgroundColor = [[UIColor colorWithWhite:0.0f alpha:0.5f] CGColor];
        fasAi.hidesWhenStopped = YES;
        
        [parent.view addSubview:fasAi];
        
        if(!operatable)
            [[UIApplication sharedApplication] beginIgnoringInteractionEvents];
        
        [fasAi startAnimating];
    }
    
    void _FasStopAcitvityIndicator(){
        
        [[UIApplication sharedApplication] endIgnoringInteractionEvents];
        
        if(fasAi != NULL)
            [fasAi stopAnimating];
        
        fasAi = NULL;
        
    }
    
    void _FasSaveVideoAtPathToSavedPhotosAlbum(const char *videoPath)
    {
        NSString *outputPath = [[NSString alloc] initWithUTF8String:videoPath];
        
        UISaveVideoAtPathToSavedPhotosAlbum(outputPath, nil, nil, nil);
    }
    
    void _FasShrinkImage(const char* srcPath, const char* dstPath, int width, int height)
    {
        UIImage *srcImage = [UIImage imageWithContentsOfFile:[[NSString alloc] initWithUTF8String:srcPath]];
        
        UIImage *dstImage = nil;
        
        CGSize imageSize = srcImage.size;
        
        CGFloat sw = imageSize.width;
        
        CGFloat sh = imageSize.height;
        
        CGSize targetSize = CGSizeMake(width, height);
        
        CGFloat targetWidth = targetSize.width;
        
        CGFloat targetHeight = targetSize.height;
        
        CGFloat scaleFactor = 0.0;
        
        CGFloat scaledWidth = targetWidth;
        
        CGFloat scaledHeight = targetHeight;
        
        CGPoint thumbnailPoint = CGPointMake(0.0,0.0);
        
        if (CGSizeEqualToSize(imageSize, targetSize) == NO)
        {
            CGFloat widthFactor = targetWidth / sw;
            
            CGFloat heightFactor = targetHeight / sh;
            
            if (widthFactor > heightFactor)
            {
                scaleFactor = widthFactor; // scale to fit height
            }
            else
            {
                scaleFactor = heightFactor; // scale to fit width
            }
            
            scaledWidth  = sw * scaleFactor;
            
            scaledHeight = sh * scaleFactor;
            
            // center the image
            if (widthFactor > heightFactor)
            {
                thumbnailPoint.y = (targetHeight - scaledHeight) * 0.5;
            }
            else
            {
                if (widthFactor < heightFactor)
                {
                    thumbnailPoint.x = (targetWidth - scaledWidth) * 0.5;
                }
            }
        }
        
        UIGraphicsBeginImageContext(targetSize); // this will crop
        
        CGRect thumbnailRect = CGRectZero;
        
        thumbnailRect.origin = thumbnailPoint;
        
        thumbnailRect.size.width  = scaledWidth;
        
        thumbnailRect.size.height = scaledHeight;
        
        [srcImage drawInRect:thumbnailRect];
        
        dstImage = UIGraphicsGetImageFromCurrentImageContext();
        
        if(dstImage == nil)
        {
            NSLog(@"could not scale image");
        }
        
        //pop the context to get back to the default
        UIGraphicsEndImageContext();
        
        NSData *imageData = UIImagePNGRepresentation(dstImage);
        
        [imageData writeToFile:[[NSString alloc] initWithUTF8String:dstPath] atomically:YES];
        
    }
}
