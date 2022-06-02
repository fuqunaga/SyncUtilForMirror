# SyncUtil For Mirror
Sync Utilities For [Mirror](https://github.com/vis2k/Mirror)  
  

## Install Dependency
 - [Mirror via AssetStore](https://assetstore.unity.com/packages/tools/network/mirror-129321)

## Installation

**Edit > ProjectSettings... > Package Manager > Scoped Registries**

Enter the following and click the Save button.

```
"name": "fuqunaga",
"url": "https://registry.npmjs.com",
"scopes": [ "ga.fuquna" ]
```
![](Documentation~/2022-04-12-17-29-38.png)


**Window > Package Manager**

Select `MyRegistries` in `Packages:`

![](Documentation~/2022-04-12-17-40-26.png)

Select `SyncUtil For Mirror` and click the Install button
<!-- ![](Documentation~/2022-04-12-18-04-29.png) -->

## How to run Examples

1. Add all scene files(*.unity) to `Scenes In Build` of Build Settings.
1. Open and run SyncUtilExamples scene. this is an example scene launcher.

## Syncing Parameters

### Primitive Members
[![](http://img.youtube.com/vi/RoescKd70Fs/0.jpg)](https://www.youtube.com/watch?v=RoescKd70Fs)

### Behaviour.enabled, GameObject active
[![](http://img.youtube.com/vi/C39lSQUmYyY/0.jpg)](https://www.youtube.com/watch?v=C39lSQUmYyY)

## Random Per Instance
[![](http://img.youtube.com/vi/Jml_K5ipCZI/0.jpg)](https://www.youtube.com/watch?v=Jml_K5ipCZI)

## Spawner, ServerOrStandAlone
[![](http://img.youtube.com/vi/_fBlCKlia4A/0.jpg)](https://www.youtube.com/watch?v=_fBlCKlia4A)  
auto register spawnable prefab  
  
[![](http://img.youtube.com/vi/2qMK0PuPIHY/0.jpg)](https://www.youtube.com/watch?v=2qMK0PuPIHY)  
Spawner: spawn prefabs  
ServerOrStandAlone: disable children on the client  
  
## Scene Load Helper
[![](http://img.youtube.com/vi/RQmx5Dr5_MQ/0.jpg)](https://www.youtube.com/watch?v=RQmx5Dr5_MQ)  
unload online/offline scene on hierarchy when application play  

## Latency Check, Delay Rendering
[![](http://img.youtube.com/vi/WXi7Jfautpw/0.jpg)](https://www.youtube.com/watch?v=WXi7Jfautpw)  
dynamic delay rendering according to network latency 

## LockStep, LockStepGPU
[![](http://img.youtube.com/vi/NmddY56bRPk/0.jpg)](https://www.youtube.com/watch?v=NmddY56bRPk)  


# References
 - https://github.com/fuqunaga/PrefsGUISyncForMirror