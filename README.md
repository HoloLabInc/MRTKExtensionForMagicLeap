# MRTKExtensionForMagicLeap
This is Mixed Reality Toolkit (MRTK) extension for Magic Leap.

## Demo Video
[![Demo video](http://img.youtube.com/vi/io1a-ShNIDY/0.jpg)](http://www.youtube.com/watch?v=io1a-ShNIDY)

# Prerequisites
- Unity 2019.2.x or higher
- Magic Leap One
- Magic Leap Unity Package 0.22.0

# Getting Started

## 1.A. Clone this repository
```
> git clone git@github.com:HoloLabInc/MRTKExtensionForMagicLeap.git --recursive
> cd MRTKExtensionForMagicLeap
> External\createSymlink.bat
```

Open MRTKExtensionForMagicLeap project with Unity 2019.2.x or higher

## 1.B. Import unitypackages
Create project with Unity 2019.2.x or higher

### Import MRTK v2
Download and import MRTK v2 unitypackages.  
(https://github.com/microsoft/MixedRealityToolkit-Unity/releases)

### Import MRTKExtensionForMagicLeap
Download and import the latest MRTKExtensionForMagicLeap unitypackage.  
(https://github.com/HoloLabInc/MRTKExtensionForMagicLeap/releases)

## 2. Import Magic Leap unitypackage
Import MagicLeap.unitypackage, which you can download with Magic Leap PackageManager.

## 3. Project setup
### Basic setup
Setup Unity project according to the following documentations.

- https://creator.magicleap.com/learn/guides/get-started-developing-in-unity
- https://creator.magicleap.com/learn/guides/unity-setup

1. Open "Edit" > "Preferences" > "External Tools" and specify Lumin SDK version folder.
1. Import the XR Management Package
1. Specify Magic Leap Loader in "Project Settings" > "XR Plugin Management"
1. Change Api Compatibility Level to .NET 4.x.


### Force multipass rendering
Open "Edit" > "Project Settings" > "XR Plugin Management" > "Magic Leap Settings" and check "Fore Multipass".

![image](https://user-images.githubusercontent.com/4415085/69318279-31daee00-0c80-11ea-8566-7611a6d371c5.png)


### Capability setting
Check the following capabilities.
- ControllerPose
- GesturesConfig
- GesturesSubscribe
- LowLatencyLightwear

![Capabilities](https://user-images.githubusercontent.com/4415085/69318178-f0e2d980-0c7f-11ea-9631-deafcf0d7792.png)

## Example scene
The example scenes are under `MixedRealityToolkit.ThirdParty/MagicLeapInput/Scenes`.

# Known Issue
Canvas UI does not work well.

# Author
Furuta, Yusuke ([@tarukosu](https://twitter.com/tarukosu))

# License
MIT