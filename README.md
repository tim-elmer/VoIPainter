# VoIPainter

> This project is currently on hold, as I no longer have access to a CUCM system. It should, however, be functionally complete, and bug reports will be addressed if possible.

## Description
VoIPainter is a simple Windows utility designed to change the background on a user's Cisco VoIP phone, without requiring access to the CUCM administrative backend.

## Requirements / Caveats
- The phone must be _owned by_ the executing user in CUCM.
- _Personalization must be enabled_ on the phone in CUCM.
- _Web access must be enabled_ on the phone in CUCM (the commands are sent via HTTP).
- The executing user must have _local administrative privileges_; VoIPainter will open a socket on TCP port 80, which requires administrative rights.
- _caveat:_ Key Expansion Modules / Sidecars are currently not supported. They are listed in the specification, but it's not trivially obvious how to specify deployment of a KEM over the phone's wallpaper itself, if possible at all, and I don't have one to test with.

## Use
> Note that you may apply a background image, a ringtone, or both simultaneously.

### Applying a Background Image
> Note that uncluttered, lower contrast images work best.
1. Click the "Browse Image" button to select an image.
1. Enter [required settings](#required-settings)
1. Click "Apply"

### Applying a Ringtone
> Note that the chosen media file will automatically be trimmed to the first 20 seconds (2 seconds on older phones). If you wish to use a specific part of a longer file, you can first cut it down using an audio editor such as [Audacity](https://www.audacityteam.org/).
1. Click the "Browse Ringtone" button to select an audio file.
1. Enter [required settings](#required-settings)
1. Click "Apply"

### Required Settings
| Setting | Explanation |
| ---: | --- |
| Phone IP | The IP address of your phone. This can typically be acquired from the phone's settings menu under "Phone Information", "Status", or similar. It should be in the form `0.0.0.0`. This can be acquired from the phone's settings menu under "Phone Information", "Status", or similar. |
| Username | Your domain username |
| Password | Your domain password |
| Phone Model | The model of your phone. If you are unsure, the model is typically labeled on the back/bottom of the device. |

## Settings
| Setting | Explanation |
| ---: | --- |
| Resize Mode | How under/oversized images will be treated: Stretch: The image will be stretched to fit the screen; **Crop (recommended)**: The largest dimension will be cropped on the center of the image to fit the screen; Center: The image will be scaled to fit, and centered on the screen.|
| Target Contrast | The maximum contrast that images should have. **The default value of `0.6` is recommended.** |
| Automatically Duck Contrast | If images with excessive contrast should have their contrast lowered. |
| Use Contrast Box | Rather than lowering the contrast of the whole image, only lower a box behind the lines. |
| Ringtone Fade Out | How long the ringtone will fade out at the end. To not fade out, set to `0`. Must be between `0` and `20` seconds. |
