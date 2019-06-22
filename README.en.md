# Fake3DViewer
A Unity application that displays images and videos of RGB image \+ depth map stereoscopically on the Looking Glass.  

<img src="https://github.com/SolidUsagi/Fake3DViewer/blob/master/Assets/StreamingAssets/Images/Fuyu1_180LR.jpg" width="512">

## System requirements
- Windows
- [The Looking Glass](https://lookingglassfactory.com/)

## Usage
Drag and drop an image file or video file of RGB image \+ depth map and it will be displayed on the Looking Glass.  
Dropped images and videos are registered in the playlist, and images and videos in the playlist can be displayed by switching in order with the left and right buttons of the Looking Glass, or cursor keys.  
Press the Circle button or Space key to start the slide show.  

The playlist is stored in the following XML file.  
C:\\Users\\<user_name>\\AppData\\LocalLow\\SolidUsagi\\Fake3DViewer\\Defaults.xml  
When images and videos are displayed, pressing the Delete key will remove them from the playlist.  

Note that drag and drop is not possible when running on the Unity editor.  

## Image format
In addition to normal rectangular images, it supports the Equirectangular format. After dropping the file, press the F2 key to switch to the proper format.  
\[ F2 \]: rectangular -> Equirectangular \(180 degrees\) -> Equirectangular \(360 degrees\) -> rectangular -> ...  

The direction of the RGB image and depth map can be switched with the F3 key, and the order can be switched with the F4 key.  
\[ F3 \]: SBS \(Side by Side\) -> TAB \(Top and Bottom\) -> SBS -> ...  
\[ F4 \]: Each time you press it, the RGB image and depth map will be switched.  

The direction of depth in the depth map can be switched with the F1 key.  
\[ F1 \]: Front positive -> Front negative -> Front positive -> ...  

Image format information is recorded in the playlist.  

## Multiple files
Drop both at once if you have an RGB image and a depth map in separate files \(not possible for video files\).  
At this time, if either file name contains the string "depth", it is regarded as a depth map file. If it is not included in either, drop them and then press F4 key to switch it to the correct display.  

If the RGB image file and the depth map file are in the same folder and the depth map file name is the RGB image file name with the prefix "depth_" or the suffix "_depth" added, you only need to drop one or the other file.  
For example, if the RGB image file name is "sample_image", the depth map file name should be "depth_sample_image" or "sample_image_depth".  

## Mouse operation
You can move the displayed image / video by dragging it. You can zoom in / out with the mouse wheel.  
Double-click to return to the initial position and initial magnification.  

## Screen adjustment
You can adjust the position, size, and depth length of the screen that displays images and videos placed in Holoplay Capture.  
These adjustments are recorded in the playlist. Adjustment values are individual for each image and video.  

Turn the mouse wheel while pressing the following keys. If you hold down each key and double click, it will return to the initial value.  
\[ Z \]: Screen position \(front to back\)  
\[ S \]: Screen size  
\[ D \]: Depth length \(adjustment of three-dimensional effect\)  

## License
This software is released under the MIT license.  
https://github.com/SolidUsagi/Fake3DViewer/blob/master/LICENSE  

## [The HoloPlay Unity SDK](https://docs.lookingglassfactory.com/Unity/)
Copyright 2017-18 Looking Glass Factory Inc. All rights reserved.  
https://github.com/SolidUsagi/Fake3DViewer/blob/master/Assets/Holoplay/License.pdf  

## [UnityWindowsFileDrag&Drop](https://github.com/Bunny83/UnityWindowsFileDrag-Drop)
Copyright (c) 2018 Markus Göbel (Bunny83)  
https://github.com/Bunny83/UnityWindowsFileDrag-Drop/blob/master/LICENSE  

## [PanoramaVideoWithUnity](https://github.com/makoto-unity/PanoramaVideoWithUnity)
Copyright (c) 2015 Makoto Ito  
https://github.com/makoto-unity/PanoramaVideoWithUnity/blob/master/LICENSE  
I used Sphere100.fbx.  
