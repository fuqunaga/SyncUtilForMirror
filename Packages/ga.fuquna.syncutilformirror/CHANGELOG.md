## [1.6.1](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.6.0...v1.6.1) (2024-10-21)


### Bug Fixes

* Build error after Mirror90 in SyncParamManager ([300eae7](https://github.com/fuqunaga/SyncUtilForMirror/commit/300eae70f0e72ee73d58d3bf219d3e794ee4a36a))

# [1.6.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.5.0...v1.6.0) (2024-03-27)


### Features

* add NetworkManagerController.StartedBootType ([218d463](https://github.com/fuqunaga/SyncUtilForMirror/commit/218d46320337ca74bffb8020ab8de4d05f692fd7))

# [1.5.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.4.3...v1.5.0) (2024-02-14)


### Bug Fixes

* LifeGame warning ([ff8316d](https://github.com/fuqunaga/SyncUtilForMirror/commit/ff8316dda1ae413b0540f3ae6cbaf8319a80affa))


### Features

* **LockStep:** add delayStep, processDelayStepInterval ([7ec4273](https://github.com/fuqunaga/SyncUtilForMirror/commit/7ec42731e3979e87ca25561b36ed2b0033052684))

## [1.4.3](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.4.2...v1.4.3) (2023-12-22)


### Bug Fixes

* error when LockStep.OnMissingCatchUpServer is not registered ([64e8f8d](https://github.com/fuqunaga/SyncUtilForMirror/commit/64e8f8df8b4cdc3929b78a160afc3615cc5cd6a8))

## [1.4.2](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.4.1...v1.4.2) (2023-11-28)


### Bug Fixes

* ClientHeartBeat shows last recieved frame ([ac84f54](https://github.com/fuqunaga/SyncUtilForMirror/commit/ac84f540a7f80dfedf1f542d309af02eea49cc6f))
* Shortened ClientHeatBeat.ToString() ([074134e](https://github.com/fuqunaga/SyncUtilForMirror/commit/074134e2bd52c4629d117e18b939c9e4835dec0d))

## [1.4.1](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.4.0...v1.4.1) (2023-10-16)


### Bug Fixes

* ClientInvisibility-attached objects were not displayed in scenes with ClientInvisibilityManagement. ([f168400](https://github.com/fuqunaga/SyncUtilForMirror/commit/f16840035654988100948a0a07ba6e77d74cb900))

# [1.4.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.3.0...v1.4.0) (2023-04-28)


### Features

* **LockStep:** The function to generate buffer hashes is now async ([119d470](https://github.com/fuqunaga/SyncUtilForMirror/commit/119d4701c010cabcae49bbeb092fbbd88be80a39))

# [1.3.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.2.1...v1.3.0) (2023-03-10)


### Bug Fixes

* SyncParam supports Array/List [#2](https://github.com/fuqunaga/SyncUtilForMirror/issues/2) ([c74c6a2](https://github.com/fuqunaga/SyncUtilForMirror/commit/c74c6a25f8a01e99e1ba02c6c62e9b93b45f5a24))


### Features

* **upgrade:** Mirror73 ([c53772b](https://github.com/fuqunaga/SyncUtilForMirror/commit/c53772b625d61095bf2f42b88cb16b7248b2ae52))

## [1.2.1](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.2.0...v1.2.1) (2022-10-10)


### Bug Fixes

* warning that appeared when ConsistencyChecker was generated multiple times. ([b7ec7cb](https://github.com/fuqunaga/SyncUtilForMirror/commit/b7ec7cbf16c7988ad2aa66ce66d77e6d89140a40))

# [1.2.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.1.2...v1.2.0) (2022-08-24)


### Features

* SyncNet.Spawn() can now specify space. ([ab702fd](https://github.com/fuqunaga/SyncUtilForMirror/commit/ab702fdb65bc0ed91f8ec3247b19948540673108))

## [1.1.2](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.1.1...v1.1.2) (2022-08-23)


### Bug Fixes

* build error SyncNetworkManager.OnValidate() move inside UNITY_EDITOR ([f2758ff](https://github.com/fuqunaga/SyncUtilForMirror/commit/f2758ff478a6081c0e97095526bfda12d3771cbb))

## [1.1.1](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.1.0...v1.1.1) (2022-08-23)


### Bug Fixes

* OnlineOfflineSceneLoadHelper Debug.Log() ([57d1bd1](https://github.com/fuqunaga/SyncUtilForMirror/commit/57d1bd1174bf8fcfc9f96308de989a33d7278380))

# [1.1.0](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.0.2...v1.1.0) (2022-08-23)


### Bug Fixes

*  error that appears after Play on editor. ([3ae0ced](https://github.com/fuqunaga/SyncUtilForMirror/commit/3ae0ced6820c5c3c39f92fc7da99f7ae1a228c36))


### Features

* SyncTime will be destroyed when SyncNetworkManager doesn't exist ([cde27ff](https://github.com/fuqunaga/SyncUtilForMirror/commit/cde27ffacc4393181c7638324a1708fe545b4647))

## [1.0.2](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.0.1...v1.0.2) (2022-07-13)


### Bug Fixes

* NetworkManagerController doesn't RequireComponent NetworkManager now ([38fc91b](https://github.com/fuqunaga/SyncUtilForMirror/commit/38fc91b49635fea7e2611ff5c6766ce5cafb3601))

## [1.0.1](https://github.com/fuqunaga/SyncUtilForMirror/compare/v1.0.0...v1.0.1) (2022-07-11)


### Bug Fixes

* assertion when LockStep.SetFunc returns true ([f878077](https://github.com/fuqunaga/SyncUtilForMirror/commit/f878077412e7c3ebdbb373d7921ccff31318965b))
* OnlineOfflineSceneLoadHelper didn't work ([51c6719](https://github.com/fuqunaga/SyncUtilForMirror/commit/51c67196033dc144191472173f16bd591612b431))

## 1.0.0 (2022-06-03)

* first release ([9a04fd7](https://github.com/fuqunaga/SyncUtilForMirror/commit/9a04fd701cdfd4db935d72499a7239fbd1e6ce96))
