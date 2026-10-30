#import <UIKit/UIKit.h>

// Called from Haptics.cs on iOS. style: 0 light, 1 medium, 2 heavy.
extern "C" void JD_HapticImpact(int style)
{
    UIImpactFeedbackStyle s = style == 0 ? UIImpactFeedbackStyleLight
                            : style == 1 ? UIImpactFeedbackStyleMedium
                                         : UIImpactFeedbackStyleHeavy;
    dispatch_async(dispatch_get_main_queue(), ^{
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:s];
        [generator prepare];
        [generator impactOccurred];
    });
}
