// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.PlatformSupport;
using UnityEditor.Presets;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Modules;
using UnityEditorInternal.VR;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GraphicsDeviceType = UnityEngine.Rendering.GraphicsDeviceType;
using TargetAttributes = UnityEditor.BuildTargetDiscovery.TargetAttributes;
using UnityEngine.Rendering;

// ************************************* READ BEFORE EDITING **************************************
//
// DO NOT COPY/PASTE! Please do not have the same setting more than once in the code.
// If a setting for one platform needs to be exposed to more platforms,
// change the if statements so the same lines of code are executed for both platforms,
// instead of copying the lines into multiple code blocks.
// This ensures that if we change labels, or headers, or the order of settings, it will remain
// consistent without having to remember to make the same change multiple places. THANK YOU!
//
// ADD_NEW_PLATFORM_HERE: review this file

namespace UnityEditor
{
    [CustomEditor(typeof(PlayerSettings))]
    internal partial class PlayerSettingsEditor : Editor
    {
        class Styles
        {
            public static readonly GUIStyle categoryBox = new GUIStyle(EditorStyles.helpBox);
            static Styles()
            {
                categoryBox.padding.left = 4;
            }
        }

        class SettingsContent
        {
            public static readonly GUIContent frameTimingStatsWebGLWarning = EditorGUIUtility.TrTextContent("Frame timing stats are supported in WebGL 2 only. Uncheck 'Automatic Graphics API' if it's set and remove the WebGL 1 API.");
            public static readonly GUIContent colorSpaceAndroidWarning = EditorGUIUtility.TrTextContent("Linear colorspace requires OpenGL ES 3.0 or Vulkan, uncheck 'Automatic Graphics API' to remove OpenGL ES 2 API, Blit Type for non-SRP projects must be Always Blit or Auto");
            public static readonly GUIContent colorSpaceWebGLWarning = EditorGUIUtility.TrTextContent("Linear colorspace requires WebGL 2.0, uncheck 'Automatic Graphics API' to remove WebGL 1.0 API. WARNING: If DXT sRGB is not supported by the browser, texture will be decompressed");
            public static readonly GUIContent lightmapEncodingWebGLWarning = EditorGUIUtility.TrTextContent("High quality lightmap encoding requires WebGL 2 only. Uncheck 'Automatic Graphics API' if it's set and remove the WebGL 1 API.");
            public static readonly GUIContent colorSpaceIOSWarning = EditorGUIUtility.TrTextContent("Linear colorspace requires Metal API only. Uncheck 'Automatic Graphics API' and remove OpenGL ES 2/3 APIs.");
            public static readonly GUIContent recordingInfo = EditorGUIUtility.TrTextContent("Reordering the list will switch editor to the first available platform");
            public static readonly GUIContent sharedBetweenPlatformsInfo = EditorGUIUtility.TrTextContent("* Shared setting between multiple platforms.");

            public static readonly GUIContent cursorHotspot = EditorGUIUtility.TrTextContent("Cursor Hotspot");
            public static readonly GUIContent defaultCursor = EditorGUIUtility.TrTextContent("Default Cursor");
            public static readonly GUIContent vertexChannelCompressionMask = EditorGUIUtility.TrTextContent("Vertex Compression*", "Select which vertex channels should be compressed. Compression can save memory and bandwidth, but precision will be lower.");

            public static readonly GUIContent resolutionPresentationTitle = EditorGUIUtility.TrTextContent("Resolution and Presentation");
            public static readonly GUIContent resolutionTitle = EditorGUIUtility.TrTextContent("Resolution");
            public static readonly GUIContent orientationTitle = EditorGUIUtility.TrTextContent("Orientation");
            public static readonly GUIContent allowedOrientationTitle = EditorGUIUtility.TrTextContent("Allowed Orientations for Auto Rotation");
            public static readonly GUIContent multitaskingSupportTitle = EditorGUIUtility.TrTextContent("Multitasking Support");
            public static readonly GUIContent statusBarTitle = EditorGUIUtility.TrTextContent("Status Bar");
            public static readonly GUIContent standalonePlayerOptionsTitle = EditorGUIUtility.TrTextContent("Standalone Player Options");
            public static readonly GUIContent debuggingCrashReportingTitle = EditorGUIUtility.TrTextContent("Debugging and crash reporting");
            public static readonly GUIContent debuggingTitle = EditorGUIUtility.TrTextContent("Debugging");
            public static readonly GUIContent crashReportingTitle = EditorGUIUtility.TrTextContent("Crash Reporting");
            public static readonly GUIContent otherSettingsTitle = EditorGUIUtility.TrTextContent("Other Settings");
            public static readonly GUIContent renderingTitle = EditorGUIUtility.TrTextContent("Rendering");
            public static readonly GUIContent vulkanSettingsTitle = EditorGUIUtility.TrTextContent("Vulkan Settings");
            public static readonly GUIContent identificationTitle = EditorGUIUtility.TrTextContent("Identification");
            public static readonly GUIContent macAppStoreTitle = EditorGUIUtility.TrTextContent("Mac App Store Options");
            public static readonly GUIContent configurationTitle = EditorGUIUtility.TrTextContent("Configuration");
            public static readonly GUIContent optimizationTitle = EditorGUIUtility.TrTextContent("Optimization");
            public static readonly GUIContent loggingTitle = EditorGUIUtility.TrTextContent("Stack Trace*");
            public static readonly GUIContent legacyTitle = EditorGUIUtility.TrTextContent("Legacy");
            public static readonly GUIContent publishingSettingsTitle = EditorGUIUtility.TrTextContent("Publishing Settings");

            public static readonly GUIContent bakeCollisionMeshes = EditorGUIUtility.TrTextContent("Prebake Collision Meshes*", "Bake collision data into the meshes on build time");
            public static readonly GUIContent keepLoadedShadersAlive = EditorGUIUtility.TrTextContent("Keep Loaded Shaders Alive*", "Prevents shaders from being unloaded");
            public static readonly GUIContent preloadedAssets = EditorGUIUtility.TrTextContent("Preloaded Assets*", "Assets to load at start up in the player and kept alive until the player terminates");
            public static readonly GUIContent stripEngineCode = EditorGUIUtility.TrTextContent("Strip Engine Code*", "Strip Unused Engine Code - Note that byte code stripping of managed assemblies is always enabled for the IL2CPP scripting backend.");
            public static readonly GUIContent iPhoneScriptCallOptimization = EditorGUIUtility.TrTextContent("Script Call Optimization*");
            public static readonly GUIContent enableInternalProfiler = EditorGUIUtility.TrTextContent("Enable Internal Profiler* (Deprecated)", "Internal profiler counters should be accessed by scripts using UnityEngine.Profiling::Profiler API.");
            public static readonly GUIContent stripUnusedMeshComponents = EditorGUIUtility.TrTextContent("Optimize Mesh Data*", "Remove unused mesh components");
            public static readonly GUIContent mipStripping = EditorGUIUtility.TrTextContent("Texture MipMap Stripping*", "Remove unused texture levels from package builds, reducing package size on disk. Limits the texture quality settings to the highest mip that was included during the build.");
            public static readonly GUIContent enableFrameTimingStats = EditorGUIUtility.TrTextContent("Frame Timing Stats", "Enable gathering of CPU/GPU frame timing statistics.");
            public static readonly GUIContent useOSAutoRotation = EditorGUIUtility.TrTextContent("Use Animated Autorotation", "If set OS native animated autorotation method will be used. Otherwise orientation will be changed immediately.");
            public static readonly GUIContent defaultScreenWidth = EditorGUIUtility.TrTextContent("Default Screen Width");
            public static readonly GUIContent defaultScreenHeight = EditorGUIUtility.TrTextContent("Default Screen Height");
            public static readonly GUIContent macRetinaSupport = EditorGUIUtility.TrTextContent("Mac Retina Support");
            public static readonly GUIContent runInBackground = EditorGUIUtility.TrTextContent("Run In Background*");
            public static readonly GUIContent defaultScreenOrientation = EditorGUIUtility.TrTextContent("Default Orientation*");
            public static readonly GUIContent allowedAutoRotateToPortrait = EditorGUIUtility.TrTextContent("Portrait");
            public static readonly GUIContent allowedAutoRotateToPortraitUpsideDown = EditorGUIUtility.TrTextContent("Portrait Upside Down");
            public static readonly GUIContent allowedAutoRotateToLandscapeRight = EditorGUIUtility.TrTextContent("Landscape Right");
            public static readonly GUIContent allowedAutoRotateToLandscapeLeft = EditorGUIUtility.TrTextContent("Landscape Left");
            public static readonly GUIContent UIRequiresFullScreen = EditorGUIUtility.TrTextContent("Requires Fullscreen");
            public static readonly GUIContent UIStatusBarHidden = EditorGUIUtility.TrTextContent("Status Bar Hidden");
            public static readonly GUIContent UIStatusBarStyle = EditorGUIUtility.TrTextContent("Status Bar Style");
            public static readonly GUIContent useMacAppStoreValidation = EditorGUIUtility.TrTextContent("Mac App Store Validation");
            public static readonly GUIContent macAppStoreCategory = EditorGUIUtility.TrTextContent("Category", "'LSApplicationCategoryType'");
            public static readonly GUIContent fullscreenMode = EditorGUIUtility.TrTextContent("Fullscreen Mode ", " Not all platforms support all modes");
            public static readonly GUIContent exclusiveFullscreen = EditorGUIUtility.TrTextContent("Exclusive Fullscreen");
            public static readonly GUIContent fullscreenWindow = EditorGUIUtility.TrTextContent("Fullscreen Window");
            public static readonly GUIContent maximizedWindow = EditorGUIUtility.TrTextContent("Maximized Window");
            public static readonly GUIContent windowed = EditorGUIUtility.TrTextContent("Windowed");
            public static readonly GUIContent displayResolutionDialogEnabledLabel = EditorGUIUtility.TrTextContent("Enabled (Deprecated)");
            public static readonly GUIContent displayResolutionDialogHiddenLabel = EditorGUIUtility.TrTextContent("Hidden by Default (Deprecated)");
            public static readonly GUIContent displayResolutionDialogDeprecationWarning = EditorGUIUtility.TrTextContent("The Display Resolution Dialog has been deprecated and will be removed in a future version.");
            public static readonly GUIContent visibleInBackground = EditorGUIUtility.TrTextContent("Visible In Background");
            public static readonly GUIContent allowFullscreenSwitch = EditorGUIUtility.TrTextContent("Allow Fullscreen Switch*");
            public static readonly GUIContent useFlipModelSwapChain = EditorGUIUtility.TrTextContent("Use DXGI flip model swapchain for D3D11", "Disable this option to fallback to Windows 7-style BitBlt model. Using flip model (leaving this option enabled) ensures the best performance. This setting affects only D3D11 graphics API.");
            public static readonly GUIContent flipModelSwapChainWarning = EditorGUIUtility.TrTextContent("Disabling DXGI flip model swapchain will result in Unity falling back to the slower and less efficient BitBlt model. See documentation for more information.");
            public static readonly GUIContent use32BitDisplayBuffer = EditorGUIUtility.TrTextContent("Use 32-bit Display Buffer*", "If set Display Buffer will be created to hold 32-bit color values. Use it only if you see banding, as it has performance implications.");
            public static readonly GUIContent disableDepthAndStencilBuffers = EditorGUIUtility.TrTextContent("Disable Depth and Stencil*");
            public static readonly GUIContent preserveFramebufferAlpha = EditorGUIUtility.TrTextContent("Render Over Native UI*", "Enable this option ONLY if you want Unity to render on top of the native Android or iOS UI.");
            public static readonly GUIContent iosShowActivityIndicatorOnLoading = EditorGUIUtility.TrTextContent("Show Loading Indicator");
            public static readonly GUIContent androidShowActivityIndicatorOnLoading = EditorGUIUtility.TrTextContent("Show Loading Indicator");
            public static readonly GUIContent actionOnDotNetUnhandledException = EditorGUIUtility.TrTextContent("On .Net UnhandledException*");
            public static readonly GUIContent logObjCUncaughtExceptions = EditorGUIUtility.TrTextContent("Log Obj-C Uncaught Exceptions*");
            public static readonly GUIContent enableCrashReportAPI = EditorGUIUtility.TrTextContent("Enable CrashReport API*");
            public static readonly GUIContent activeColorSpace = EditorGUIUtility.TrTextContent("Color Space*");
            public static readonly GUIContent colorGamut = EditorGUIUtility.TrTextContent("Color Gamut*");
            public static readonly GUIContent colorGamutForMac = EditorGUIUtility.TrTextContent("Color Gamut For Mac*");
            public static readonly GUIContent metalForceHardShadows = EditorGUIUtility.TrTextContent("Force hard shadows on Metal*");
            public static readonly GUIContent metalAPIValidation = EditorGUIUtility.TrTextContent("Metal API Validation*", "When enabled, additional binding state validation is applied.");
            public static readonly GUIContent metalFramebufferOnly = EditorGUIUtility.TrTextContent("Metal Write-Only Backbuffer", "Set framebufferOnly flag on backbuffer. This prevents readback from backbuffer but enables some driver optimizations.");
            public static readonly GUIContent framebufferDepthMemorylessMode = EditorGUIUtility.TrTextContent("Memoryless Depth", "Memoryless mode of framebuffer depth");
            public static readonly GUIContent[] memorylessModeNames = { EditorGUIUtility.TrTextContent("Unused"), EditorGUIUtility.TrTextContent("Forced"), EditorGUIUtility.TrTextContent("Automatic") };
            public static readonly GUIContent vulkanEnableSetSRGBWrite = EditorGUIUtility.TrTextContent("SRGB Write Mode*", "If set, enables Graphics.SetSRGBWrite() for toggling sRGB write mode during the frame but may decrease performance especially on tiled GPUs.");
            public static readonly GUIContent vulkanNumSwapchainBuffers = EditorGUIUtility.TrTextContent("Number of swapchain buffers*");
            public static readonly GUIContent vulkanEnableLateAcquireNextImage = EditorGUIUtility.TrTextContent("Acquire swapchain image late as possible*", "If set, renders to a staging image to delay acquiring the swapchain buffer.");
            public static readonly GUIContent vulkanEnableCommandBufferRecycling = EditorGUIUtility.TrTextContent("Recycle command buffers*", "When enabled, command buffers are recycled after they have been executed as opposed to being freed.");
            public static readonly GUIContent mTRendering = EditorGUIUtility.TrTextContent("Multithreaded Rendering*");
            public static readonly GUIContent staticBatching = EditorGUIUtility.TrTextContent("Static Batching");
            public static readonly GUIContent dynamicBatching = EditorGUIUtility.TrTextContent("Dynamic Batching");
            public static readonly GUIContent graphicsJobsNonExperimental = EditorGUIUtility.TrTextContent("Graphics Jobs");
            public static readonly GUIContent graphicsJobsExperimental = EditorGUIUtility.TrTextContent("Graphics Jobs (Experimental)");
            public static readonly GUIContent graphicsJobsMode = EditorGUIUtility.TrTextContent("Graphics Jobs Mode");
            public static readonly GUIContent applicationIdentifierWarning = EditorGUIUtility.TrTextContent("Invalid characters have been removed from the Application Identifier.");
            public static readonly GUIContent applicationIdentifierError = EditorGUIUtility.TrTextContent("The Application Identifier must follow the convention 'com.YourCompanyName.YourProductName' and must contain only alphanumeric and hyphen characters.");
            public static readonly GUIContent packageNameError = EditorGUIUtility.TrTextContent("The Package Name must follow the convention 'com.YourCompanyName.YourProductName' and must contain only alphanumeric and underscore characters. Each segment must start with an alphabetical character.");
            public static readonly GUIContent applicationBuildNumber = EditorGUIUtility.TrTextContent("Build");
            public static readonly GUIContent appleDeveloperTeamID = EditorGUIUtility.TrTextContent("iOS Developer Team ID", "Developers can retrieve their Team ID by visiting the Apple Developer site under Account > Membership.");
            public static readonly GUIContent useOnDemandResources = EditorGUIUtility.TrTextContent("Use on-demand resources*");
            public static readonly GUIContent gcIncremental = EditorGUIUtility.TrTextContent("Use incremental GC", "With incremental Garbage Collection, the Garbage Collector will try to time-slice the collection task into multiple steps, to avoid long GC times preventing content from running smoothly.");
            public static readonly GUIContent assemblyVersionValidation = EditorGUIUtility.TrTextContent("Assembly Version Validation", "When Mono Resolves types from a assembly that is Strong Named, versions have to match with the one already loaded.");
            public static readonly GUIContent assemblyVersionValidationEditorOnly = EditorGUIUtility.TrTextContent("Assembly Version Validation (editor only)", "When Mono Resolves types from a assembly that is Strong Named, versions have to match with the one already loaded.");
            public static readonly GUIContent accelerometerFrequency = EditorGUIUtility.TrTextContent("Accelerometer Frequency*");
            public static readonly GUIContent cameraUsageDescription = EditorGUIUtility.TrTextContent("Camera Usage Description*", "String shown to the user when requesting permission to use the device camera. Written to the NSCameraUsageDescription field in Xcode project's info.plist file");
            public static readonly GUIContent locationUsageDescription = EditorGUIUtility.TrTextContent("Location Usage Description*", "String shown to the user when requesting permission to access the device location. Written to the NSLocationWhenInUseUsageDescription field in Xcode project's info.plist file.");
            public static readonly GUIContent microphoneUsageDescription = EditorGUIUtility.TrTextContent("Microphone Usage Description*", "String shown to the user when requesting to use the device microphone. Written to the NSMicrophoneUsageDescription field in Xcode project's info.plist file");
            public static readonly GUIContent bluetoothUsageDescription = EditorGUIUtility.TrTextContent("Bluetooth Usage Description*", "String shown to the user when requesting to use the device bluetooth. Written to the NSBluetoothAlwaysUsageDescription field in Xcode project's info.plist file");
            public static readonly GUIContent muteOtherAudioSources = EditorGUIUtility.TrTextContent("Mute Other Audio Sources*");
            public static readonly GUIContent prepareIOSForRecording = EditorGUIUtility.TrTextContent("Prepare iOS for Recording");
            public static readonly GUIContent forceIOSSpeakersWhenRecording = EditorGUIUtility.TrTextContent("Force iOS Speakers when Recording");
            public static readonly GUIContent UIRequiresPersistentWiFi = EditorGUIUtility.TrTextContent("Requires Persistent WiFi*");
            public static readonly GUIContent iOSAllowHTTPDownload = EditorGUIUtility.TrTextContent("Allow downloads over HTTP (nonsecure)*");
            public static readonly GUIContent iOSURLSchemes = EditorGUIUtility.TrTextContent("Supported URL schemes*");
            public static readonly GUIContent iOSExternalAudioInputNotSupported = EditorGUIUtility.TrTextContent("Audio input from Bluetooth microphones is not supported when Mute Other Audio Sources is off.");
            public static readonly GUIContent aotOptions = EditorGUIUtility.TrTextContent("AOT Compilation Options*");
            public static readonly GUIContent require31 = EditorGUIUtility.TrTextContent("Require ES3.1");
            public static readonly GUIContent requireAEP = EditorGUIUtility.TrTextContent("Require ES3.1+AEP");
            public static readonly GUIContent require32 = EditorGUIUtility.TrTextContent("Require ES3.2");
            public static readonly GUIContent skinOnGPU = EditorGUIUtility.TrTextContent("GPU Skinning*", "Use DX11/ES3 GPU Skinning");
            public static readonly GUIContent skinOnGPUCompute = EditorGUIUtility.TrTextContent("Compute Skinning*", "Use Compute pipeline for Skinning");
            public static readonly GUIContent scriptingDefineSymbols = EditorGUIUtility.TrTextContent("Scripting Define Symbols", "Preprocessor defines passed to the C# script compiler.");
            public static readonly GUIContent scriptingDefineSymbolsApply = EditorGUIUtility.TrTextContent("Apply");
            public static readonly GUIContent scriptingDefineSymbolsApplyRevert = EditorGUIUtility.TrTextContent("Revert");
            public static readonly GUIContent scriptingDefineSymbolsCopyDefines = EditorGUIUtility.TrTextContent("Copy Defines", "Copy applied defines");
            public static readonly GUIContent suppressCommonWarnings = EditorGUIUtility.TrTextContent("Suppress Common Warnings", "Suppresses C# warnings CS0169 and CS0649.");
            public static readonly GUIContent additionalCompilerArguments = EditorGUIUtility.TrTextContent("Additional Compiler Arguments", "Additional arguments passed to the C# script compiler.");
            public static readonly GUIContent scriptingBackend = EditorGUIUtility.TrTextContent("Scripting Backend");
            public static readonly GUIContent managedStrippingLevel = EditorGUIUtility.TrTextContent("Managed Stripping Level", "If scripting backend is IL2CPP, managed stripping can't be disabled.");
            public static readonly GUIContent il2cppCompilerConfiguration = EditorGUIUtility.TrTextContent("C++ Compiler Configuration");
            public static readonly GUIContent scriptingMono2x = EditorGUIUtility.TrTextContent("Mono");
            public static readonly GUIContent scriptingIL2CPP = EditorGUIUtility.TrTextContent("IL2CPP");
            public static readonly GUIContent scriptingDefault = EditorGUIUtility.TrTextContent("Default");
            public static readonly GUIContent strippingDisabled = EditorGUIUtility.TrTextContent("Disabled");
            public static readonly GUIContent strippingLow = EditorGUIUtility.TrTextContent("Low");
            public static readonly GUIContent strippingMedium = EditorGUIUtility.TrTextContent("Medium");
            public static readonly GUIContent strippingHigh = EditorGUIUtility.TrTextContent("High");
            public static readonly GUIContent apiCompatibilityLevel = EditorGUIUtility.TrTextContent("Api Compatibility Level*");
            public static readonly GUIContent apiCompatibilityLevel_NET_2_0 = EditorGUIUtility.TrTextContent(".NET 2.0");
            public static readonly GUIContent apiCompatibilityLevel_NET_2_0_Subset = EditorGUIUtility.TrTextContent(".NET 2.0 Subset");
            public static readonly GUIContent apiCompatibilityLevel_NET_4_6 = EditorGUIUtility.TrTextContent(".NET 4.x");
            public static readonly GUIContent apiCompatibilityLevel_NET_Standard_2_0 = EditorGUIUtility.TrTextContent(".NET Standard 2.0");
            public static readonly GUIContent scriptCompilationTitle = EditorGUIUtility.TrTextContent("Script Compilation");
            public static readonly GUIContent allowUnsafeCode = EditorGUIUtility.TrTextContent("Allow 'unsafe' Code", "Allow compilation of unsafe code for predefined assemblies (Assembly-CSharp.dll, etc.)");
            public static readonly GUIContent useDeterministicCompilation = EditorGUIUtility.TrTextContent("Use Deterministic Compilation", "Compile with -deterministic compilation flag");
            public static readonly GUIContent useReferenceAssembclies = EditorGUIUtility.TrTextContent("Use Roslyn Reference Assemblies", "Skips compilation of assembly references if the metadata of the modified assembly does not change.");
            public static readonly GUIContent enableRoslynAnalyzers = EditorGUIUtility.TrTextContent("Enable Roslyn Analyzers", "User-written scripts will be compiled with Roslyn analyzer DLLs that are present in the project.");
            public static readonly GUIContent activeInputHandling = EditorGUIUtility.TrTextContent("Active Input Handling*");
            public static readonly GUIContent[] activeInputHandlingOptions = new GUIContent[] { EditorGUIUtility.TrTextContent("Input Manager (Old)"), EditorGUIUtility.TrTextContent("Input System Package (New)"), EditorGUIUtility.TrTextContent("Both") };
            public static readonly GUIContent normalMapEncodingLabel = EditorGUIUtility.TrTextContent("Normal Map Encoding");
            public static readonly GUIContent[] normalMapEncodingNames = { EditorGUIUtility.TrTextContent("XYZ"), EditorGUIUtility.TrTextContent("DXT5nm-style") };
            public static readonly GUIContent lightmapEncodingLabel = EditorGUIUtility.TrTextContent("Lightmap Encoding", "Affects the encoding scheme and compression format of the lightmaps.");
            public static readonly GUIContent[] lightmapEncodingNames = { EditorGUIUtility.TrTextContent("Low Quality"), EditorGUIUtility.TrTextContent("Normal Quality"), EditorGUIUtility.TrTextContent("High Quality") };
            public static readonly GUIContent lightmapStreamingEnabled = EditorGUIUtility.TrTextContent("Lightmap Streaming", "Only load larger lightmap mipmaps as needed to render the current game cameras. Requires texture streaming to be enabled in quality settings. This value is applied to the light map textures as they are generated.");
            public static readonly GUIContent lightmapStreamingPriority = EditorGUIUtility.TrTextContent("Streaming Priority", "Lightmap mipmap streaming priority when there's contention for resources. Positive numbers represent higher priority. Valid range is -128 to 127. This value is applied to the light map textures as they are generated.");
            public static readonly GUIContent lightmapQualityAndroidWarning = EditorGUIUtility.TrTextContent("The selected Lightmap Encoding requires OpenGL ES 3.0 or Vulkan. Uncheck 'Automatic Graphics API' and remove OpenGL ES 2 API");
            public static readonly GUIContent lightmapQualityIOSWarning = EditorGUIUtility.TrTextContent("The selected Lightmap Encoding requires Metal API only. Uncheck 'Automatic Graphics API' and remove OpenGL ES APIs.");
            public static readonly GUIContent legacyClampBlendShapeWeights = EditorGUIUtility.TrTextContent("Clamp BlendShapes (Deprecated)*", "If set, the range of BlendShape weights in SkinnedMeshRenderers will be clamped.");
            public static readonly GUIContent virtualTexturingSupportEnabled = EditorGUIUtility.TrTextContent("Virtual Texturing (Experimental)*", "Enable Virtual Texturing. This feature is experimental and not ready for production use. Changing this value requires an Editor restart.");
            public static readonly GUIContent virtualTexturingUnsupportedPlatformWarning = EditorGUIUtility.TrTextContent("The current target platform does not support Virtual Texturing. To build for this platform, uncheck Enable Virtual Texturing.");
            public static readonly GUIContent virtualTexturingUnsupportedAPI = EditorGUIUtility.TrTextContent("The target graphics API does not support Virtual Texturing. To target compatible graphics APIs, uncheck 'Auto Graphics API', and remove OpenGL ES 2/3 and OpenGLCore.");
            public static readonly GUIContent virtualTexturingUnsupportedAPIWin = EditorGUIUtility.TrTextContent("The target Windows graphics API does not support Virtual Texturing. To target compatible graphics APIs, uncheck 'Auto Graphics API for Windows', and remove OpenGL ES 2/3 and OpenGLCore.");
            public static readonly GUIContent virtualTexturingUnsupportedAPIMac = EditorGUIUtility.TrTextContent("The target Mac graphics API does not support Virtual Texturing. To target compatible graphics APIs, uncheck 'Auto Graphics API for Mac', and remove OpenGLCore.");
            public static readonly GUIContent virtualTexturingUnsupportedAPILinux = EditorGUIUtility.TrTextContent("The target Linux graphics API does not support Virtual Texturing. To target compatible graphics APIs, uncheck 'Auto Graphics API for Linux', and remove OpenGLCore.");
            public static readonly GUIContent shaderPrecisionModel = EditorGUIUtility.TrTextContent("Shader precision model*", "Mobile targets prefer lower precision by default to improve performance, but your rendering pipeline may prefer full precision by default and to optimize against lower precision cases explicitly.");
            public static readonly GUIContent[] shaderPrecisionModelOptions = { EditorGUIUtility.TrTextContent("Use platform defaults for sampler precision"), EditorGUIUtility.TrTextContent("Use full sampler precision by default, lower precision explicitly declared") };
            public static readonly GUIContent stereo360CaptureCheckbox = EditorGUIUtility.TrTextContent("360 Stereo Capture*");

            public static string undoChangedBatchingString { get { return LocalizationDatabase.GetLocalizedString("Changed Batching Settings"); } }
            public static string undoChangedGraphicsAPIString { get { return LocalizationDatabase.GetLocalizedString("Changed Graphics API Settings"); } }
            public static string undoChangedScriptingDefineString { get { return LocalizationDatabase.GetLocalizedString("Changed Scripting Define Settings"); } }
            public static string undoChangedGraphicsJobsString { get { return LocalizationDatabase.GetLocalizedString("Changed Graphics Jobs Setting"); } }
            public static string undoChangedGraphicsJobModeString { get { return LocalizationDatabase.GetLocalizedString("Changed Graphics Job Mode Setting"); } }
            public static string changeColorSpaceString { get { return LocalizationDatabase.GetLocalizedString("Changing the color space may take a significant amount of time."); } }
        }

        class RecompileReason
        {
            public static readonly string scriptingDefineSymbolsModified = "Scripting define symbols setting modified";
            public static readonly string suppressCommonWarningsModified = "Suppress common warnings setting modified";
            public static readonly string allowUnsafeCodeModified = "Allow 'unsafe' code setting modified";
            public static readonly string apiCompatibilityLevelModified = "API Compatibility level modified";
            public static readonly string useDeterministicCompilationModified = "Use deterministic compilation modified";
            public static readonly string playerSettingsModified = "Player settings modified";
            public static readonly string additionalCompilerArgumentsModified = "Additional compiler arguments modified";
        }

        PlayerSettingsSplashScreenEditor m_SplashScreenEditor;
        PlayerSettingsSplashScreenEditor splashScreenEditor
        {
            get
            {
                if (m_SplashScreenEditor == null)
                    m_SplashScreenEditor = new PlayerSettingsSplashScreenEditor(this);
                return m_SplashScreenEditor;
            }
        }

        PlayerSettingsIconsEditor m_IconsEditor;
        PlayerSettingsIconsEditor iconsEditor
        {
            get
            {
                if (m_IconsEditor == null)
                    m_IconsEditor = new PlayerSettingsIconsEditor(this);
                return m_IconsEditor;
            }
        }

        private static GraphicsJobMode[] m_GfxJobModeValues = new GraphicsJobMode[] { GraphicsJobMode.Native, GraphicsJobMode.Legacy };
        private static GUIContent[] m_GfxJobModeNames = new GUIContent[] { EditorGUIUtility.TrTextContent("Native"), EditorGUIUtility.TrTextContent("Legacy") };

        // Section and tab selection state

        SavedInt m_SelectedSection = new SavedInt("PlayerSettings.ShownSection", -1);

        BuildPlatform[] validPlatforms;
        BuildTargetGroup lastTargetGroup;

        // il2cpp
        SerializedProperty m_StripEngineCode;

        // macOS
        SerializedProperty m_ApplicationBundleVersion;
        SerializedProperty m_UseMacAppStoreValidation;
        SerializedProperty m_MacAppStoreCategory;

        // vulkan
        SerializedProperty m_VulkanNumSwapchainBuffers;
        SerializedProperty m_VulkanEnableLateAcquireNextImage;
        SerializedProperty m_VulkanEnableCommandBufferRecycling;

        // iOS, tvOS
#pragma warning disable 169
        SerializedProperty m_IPhoneApplicationDisplayName;

        SerializedProperty m_CameraUsageDescription;
        SerializedProperty m_LocationUsageDescription;
        SerializedProperty m_MicrophoneUsageDescription;
        SerializedProperty m_BluetoothUsageDescription;

        SerializedProperty m_IPhoneScriptCallOptimization;
        SerializedProperty m_AotOptions;

        SerializedProperty m_DefaultScreenOrientation;
        SerializedProperty m_AllowedAutoRotateToPortrait;
        SerializedProperty m_AllowedAutoRotateToPortraitUpsideDown;
        SerializedProperty m_AllowedAutoRotateToLandscapeRight;
        SerializedProperty m_AllowedAutoRotateToLandscapeLeft;
        SerializedProperty m_UseOSAutoRotation;
        SerializedProperty m_Use32BitDisplayBuffer;
        SerializedProperty m_PreserveFramebufferAlpha;
        SerializedProperty m_DisableDepthAndStencilBuffers;
        SerializedProperty m_iosShowActivityIndicatorOnLoading;
        SerializedProperty m_androidShowActivityIndicatorOnLoading;

        SerializedProperty m_AndroidProfiler;

        SerializedProperty m_UIRequiresPersistentWiFi;
        SerializedProperty m_UIStatusBarHidden;
        SerializedProperty m_UIRequiresFullScreen;
        SerializedProperty m_UIStatusBarStyle;

        SerializedProperty m_IOSAllowHTTPDownload;
        SerializedProperty m_SubmitAnalytics;

        SerializedProperty m_IOSURLSchemes;

        SerializedProperty m_AccelerometerFrequency;
        SerializedProperty m_useOnDemandResources;
        SerializedProperty m_MuteOtherAudioSources;
        SerializedProperty m_PrepareIOSForRecording;
        SerializedProperty m_ForceIOSSpeakersWhenRecording;

        SerializedProperty m_EnableInternalProfiler;
        SerializedProperty m_ActionOnDotNetUnhandledException;
        SerializedProperty m_LogObjCUncaughtExceptions;
        SerializedProperty m_EnableCrashReportAPI;

        SerializedProperty m_SuppressCommonWarnings;
        SerializedProperty m_AllowUnsafeCode;
        SerializedProperty m_GCIncremental;
        SerializedProperty m_AssemblyVersionValidation;

        SerializedProperty m_OverrideDefaultApplicationIdentifier;
        SerializedProperty m_ApplicationIdentifier;
        SerializedProperty m_BuildNumber;

        // General
        SerializedProperty m_CompanyName;
        SerializedProperty m_ProductName;

        // Cursor
        SerializedProperty m_DefaultCursor;
        SerializedProperty m_CursorHotspot;

        // Screen
        SerializedProperty m_DefaultScreenWidth;
        SerializedProperty m_DefaultScreenHeight;

        SerializedProperty m_ActiveColorSpace;
        SerializedProperty m_StripUnusedMeshComponents;
        SerializedProperty m_MipStripping;
        SerializedProperty m_VertexChannelCompressionMask;
        SerializedProperty m_MetalAPIValidation;
        SerializedProperty m_MetalFramebufferOnly;
        SerializedProperty m_MetalForceHardShadows;
        SerializedProperty m_FramebufferDepthMemorylessMode;

        SerializedProperty m_DefaultIsNativeResolution;
        SerializedProperty m_MacRetinaSupport;

        SerializedProperty m_UsePlayerLog;
        SerializedProperty m_KeepLoadedShadersAlive;
        SerializedProperty m_PreloadedAssets;
        SerializedProperty m_BakeCollisionMeshes;
        SerializedProperty m_ResizableWindow;
        SerializedProperty m_FullscreenMode;
        SerializedProperty m_VisibleInBackground;
        SerializedProperty m_AllowFullscreenSwitch;
        SerializedProperty m_ForceSingleInstance;
        SerializedProperty m_UseFlipModelSwapchain;

        SerializedProperty m_RunInBackground;
        SerializedProperty m_CaptureSingleScreen;

        SerializedProperty m_SupportedAspectRatios;

        SerializedProperty m_SkinOnGPU;

        // OpenGL ES 3.1+
        SerializedProperty m_RequireES31;
        SerializedProperty m_RequireES31AEP;
        SerializedProperty m_RequireES32;

        SerializedProperty m_LightmapEncodingQuality;
        SerializedProperty m_LightmapStreamingEnabled;
        SerializedProperty m_LightmapStreamingPriority;

        SerializedProperty m_HDRBitDepth;

        // Legacy
        SerializedProperty m_LegacyClampBlendShapeWeights;
        SerializedProperty m_AndroidEnableTango;
        SerializedProperty m_Enable360StereoCapture;

        SerializedProperty m_VirtualTexturingSupportEnabled;
        SerializedProperty m_ShaderPrecisionModel;

        // Scripting
        SerializedProperty m_UseDeterministicCompilation;
        SerializedProperty m_UseReferenceAssemblies;
        SerializedProperty m_ScriptingBackend;
        SerializedProperty m_EnableRoslynAnalyzers;
        SerializedProperty m_APICompatibilityLevel;
        SerializedProperty m_DefaultAPICompatibilityLevel;
        SerializedProperty m_Il2CppCompilerConfiguration;
        SerializedProperty m_ScriptingDefines;
        SerializedProperty m_AdditionalCompilerArguments;
        SerializedProperty m_StackTraceTypes;
        SerializedProperty m_ManagedStrippingLevel;
        SerializedProperty m_ActiveInputHandler;

        // Localization Cache
        string m_LocalizedTargetName;

        // reorderable lists of graphics devices, per platform
        static Dictionary<BuildTarget, ReorderableList> s_GraphicsDeviceLists = new Dictionary<BuildTarget, ReorderableList>();

        public static void SyncPlatformAPIsList(BuildTarget target)
        {
            if (!s_GraphicsDeviceLists.ContainsKey(target))
                return;
            s_GraphicsDeviceLists[target].list = PlayerSettings.GetGraphicsAPIs(target).ToList();
        }

        static ReorderableList s_ColorGamutList;

        public static void SyncColorGamuts()
        {
            s_ColorGamutList.list = PlayerSettings.GetColorGamuts().ToList();
        }

        int selectedPlatform = 0;
        int scriptingDefinesControlID = 0;

        int serializedActiveInputHandler = 0;
        string[] serializedAdditionalCompilerArguments;
        bool serializedSuppressCommonWarnings = true;
        bool serializedAllowUnsafeCode = false;
        string serializedScriptingDefines;
        ApiCompatibilityLevel serializedAPICompatibilityLevel;
        bool serializedUseDeterministicCompilation;

        List<string> scriptingDefinesList;
        bool hasScriptingDefinesBeenModified;
        ReorderableList scriptingDefineSymbolsList;

        List<string> additionalCompilerArgumentsList;
        bool hasAdditionalCompilerArgumentsBeenModified;
        ReorderableList additionalCompilerArgumentsReorderableList;

        ISettingEditorExtension[] m_SettingsExtensions;

        // Section animation state
        const int kNumberGUISections = 6;
        AnimBool[] m_SectionAnimators = new AnimBool[kNumberGUISections];
        readonly AnimBool m_ShowDefaultIsNativeResolution = new AnimBool();
        readonly AnimBool m_ShowResolution = new AnimBool();
        private static Texture2D s_WarningIcon;

        // Preset check
        bool isPreset = false;
        bool isPresetWindowOpen = false;
        bool hasPresetWindowClosed = false;
        bool scriptRecompileRequired = false;

        public SerializedProperty FindPropertyAssert(string name)
        {
            SerializedProperty property = serializedObject.FindProperty(name);
            if (property == null)
                Debug.LogError("Failed to find:" + name);
            return property;
        }

        void OnEnable()
        {
            isPreset = !AssetDatabase.Contains(target);
            validPlatforms = BuildPlatforms.instance.GetValidPlatforms(true).ToArray();

            m_StripEngineCode               = FindPropertyAssert("stripEngineCode");

            m_IPhoneScriptCallOptimization  = FindPropertyAssert("iPhoneScriptCallOptimization");
            m_AndroidProfiler               = FindPropertyAssert("AndroidProfiler");
            m_CompanyName                   = FindPropertyAssert("companyName");
            m_ProductName                   = FindPropertyAssert("productName");

            m_DefaultCursor                 = FindPropertyAssert("defaultCursor");
            m_CursorHotspot                 = FindPropertyAssert("cursorHotspot");


            m_UIRequiresFullScreen          = FindPropertyAssert("uIRequiresFullScreen");

            m_UIStatusBarHidden             = FindPropertyAssert("uIStatusBarHidden");
            m_UIStatusBarStyle              = FindPropertyAssert("uIStatusBarStyle");
            m_ActiveColorSpace              = FindPropertyAssert("m_ActiveColorSpace");
            m_StripUnusedMeshComponents     = FindPropertyAssert("StripUnusedMeshComponents");
            m_MipStripping                  = FindPropertyAssert("mipStripping");
            m_VertexChannelCompressionMask  = FindPropertyAssert("VertexChannelCompressionMask");
            m_MetalAPIValidation            = FindPropertyAssert("metalAPIValidation");
            m_MetalFramebufferOnly          = FindPropertyAssert("metalFramebufferOnly");
            m_MetalForceHardShadows         = FindPropertyAssert("iOSMetalForceHardShadows");
            m_FramebufferDepthMemorylessMode = FindPropertyAssert("framebufferDepthMemorylessMode");

            m_OverrideDefaultApplicationIdentifier = FindPropertyAssert("overrideDefaultApplicationIdentifier");
            m_ApplicationIdentifier         = FindPropertyAssert("applicationIdentifier");
            m_BuildNumber                   = FindPropertyAssert("buildNumber");

            m_ApplicationBundleVersion      = serializedObject.FindProperty("bundleVersion");
            if (m_ApplicationBundleVersion == null)
                m_ApplicationBundleVersion  = FindPropertyAssert("iPhoneBundleVersion");

            m_useOnDemandResources          = FindPropertyAssert("useOnDemandResources");
            m_AccelerometerFrequency        = FindPropertyAssert("accelerometerFrequency");

            m_MuteOtherAudioSources         = FindPropertyAssert("muteOtherAudioSources");
            m_PrepareIOSForRecording        = FindPropertyAssert("Prepare IOS For Recording");
            m_ForceIOSSpeakersWhenRecording = FindPropertyAssert("Force IOS Speakers When Recording");
            m_UIRequiresPersistentWiFi      = FindPropertyAssert("uIRequiresPersistentWiFi");
            m_IOSAllowHTTPDownload          = FindPropertyAssert("iosAllowHTTPDownload");
            m_SubmitAnalytics               = FindPropertyAssert("submitAnalytics");

            m_IOSURLSchemes                 = FindPropertyAssert("iOSURLSchemes");

            m_AotOptions                    = FindPropertyAssert("aotOptions");

            m_CameraUsageDescription        = FindPropertyAssert("cameraUsageDescription");
            m_LocationUsageDescription      = FindPropertyAssert("locationUsageDescription");
            m_MicrophoneUsageDescription    = FindPropertyAssert("microphoneUsageDescription");
            m_BluetoothUsageDescription     = FindPropertyAssert("bluetoothUsageDescription");

            m_EnableInternalProfiler        = FindPropertyAssert("enableInternalProfiler");
            m_ActionOnDotNetUnhandledException  = FindPropertyAssert("actionOnDotNetUnhandledException");
            m_LogObjCUncaughtExceptions     = FindPropertyAssert("logObjCUncaughtExceptions");
            m_EnableCrashReportAPI          = FindPropertyAssert("enableCrashReportAPI");

            m_SuppressCommonWarnings        = FindPropertyAssert("suppressCommonWarnings");
            m_AllowUnsafeCode               = FindPropertyAssert("allowUnsafeCode");
            m_GCIncremental                 = FindPropertyAssert("gcIncremental");
            m_AssemblyVersionValidation = FindPropertyAssert("assemblyVersionValidation");
            m_UseDeterministicCompilation   = FindPropertyAssert("useDeterministicCompilation");
            m_UseReferenceAssemblies        = FindPropertyAssert("useReferenceAssemblies");
            m_ScriptingBackend              = FindPropertyAssert("scriptingBackend");
            m_EnableRoslynAnalyzers         = FindPropertyAssert("enableRoslynAnalyzers");
            m_APICompatibilityLevel         = FindPropertyAssert("apiCompatibilityLevelPerPlatform");
            m_DefaultAPICompatibilityLevel  = FindPropertyAssert("apiCompatibilityLevel");
            m_Il2CppCompilerConfiguration   = FindPropertyAssert("il2cppCompilerConfiguration");
            m_ScriptingDefines              = FindPropertyAssert("scriptingDefineSymbols");
            m_StackTraceTypes               = FindPropertyAssert("m_StackTraceTypes");
            m_ManagedStrippingLevel         = FindPropertyAssert("managedStrippingLevel");
            m_ActiveInputHandler            = FindPropertyAssert("activeInputHandler");
            m_AdditionalCompilerArguments   = FindPropertyAssert("additionalCompilerArguments");

            m_DefaultScreenWidth            = FindPropertyAssert("defaultScreenWidth");
            m_DefaultScreenHeight           = FindPropertyAssert("defaultScreenHeight");
            m_RunInBackground               = FindPropertyAssert("runInBackground");

            m_DefaultScreenOrientation              = FindPropertyAssert("defaultScreenOrientation");
            m_AllowedAutoRotateToPortrait           = FindPropertyAssert("allowedAutorotateToPortrait");
            m_AllowedAutoRotateToPortraitUpsideDown = FindPropertyAssert("allowedAutorotateToPortraitUpsideDown");
            m_AllowedAutoRotateToLandscapeRight     = FindPropertyAssert("allowedAutorotateToLandscapeRight");
            m_AllowedAutoRotateToLandscapeLeft      = FindPropertyAssert("allowedAutorotateToLandscapeLeft");
            m_UseOSAutoRotation                     = FindPropertyAssert("useOSAutorotation");
            m_Use32BitDisplayBuffer                 = FindPropertyAssert("use32BitDisplayBuffer");
            m_PreserveFramebufferAlpha              = FindPropertyAssert("preserveFramebufferAlpha");
            m_DisableDepthAndStencilBuffers         = FindPropertyAssert("disableDepthAndStencilBuffers");
            m_iosShowActivityIndicatorOnLoading     = FindPropertyAssert("iosShowActivityIndicatorOnLoading");
            m_androidShowActivityIndicatorOnLoading = FindPropertyAssert("androidShowActivityIndicatorOnLoading");

            m_DefaultIsNativeResolution     = FindPropertyAssert("defaultIsNativeResolution");
            m_MacRetinaSupport              = FindPropertyAssert("macRetinaSupport");
            m_CaptureSingleScreen           = FindPropertyAssert("captureSingleScreen");
            m_SupportedAspectRatios         = FindPropertyAssert("m_SupportedAspectRatios");
            m_UsePlayerLog                  = FindPropertyAssert("usePlayerLog");

            m_KeepLoadedShadersAlive           = FindPropertyAssert("keepLoadedShadersAlive");
            m_PreloadedAssets                  = FindPropertyAssert("preloadedAssets");
            m_BakeCollisionMeshes              = FindPropertyAssert("bakeCollisionMeshes");
            m_ResizableWindow                  = FindPropertyAssert("resizableWindow");
            m_UseMacAppStoreValidation         = FindPropertyAssert("useMacAppStoreValidation");
            m_MacAppStoreCategory              = FindPropertyAssert("macAppStoreCategory");
            m_VulkanNumSwapchainBuffers        = FindPropertyAssert("vulkanNumSwapchainBuffers");
            m_VulkanEnableLateAcquireNextImage = FindPropertyAssert("vulkanEnableLateAcquireNextImage");
            m_VulkanEnableCommandBufferRecycling = FindPropertyAssert("vulkanEnableCommandBufferRecycling");
            m_FullscreenMode                   = FindPropertyAssert("fullscreenMode");
            m_VisibleInBackground              = FindPropertyAssert("visibleInBackground");
            m_AllowFullscreenSwitch            = FindPropertyAssert("allowFullscreenSwitch");
            m_SkinOnGPU                        = FindPropertyAssert("gpuSkinning");
            m_ForceSingleInstance              = FindPropertyAssert("forceSingleInstance");
            m_UseFlipModelSwapchain            = FindPropertyAssert("useFlipModelSwapchain");

            m_RequireES31                   = FindPropertyAssert("openGLRequireES31");
            m_RequireES31AEP                = FindPropertyAssert("openGLRequireES31AEP");
            m_RequireES32                   = FindPropertyAssert("openGLRequireES32");

            m_LegacyClampBlendShapeWeights = FindPropertyAssert("legacyClampBlendShapeWeights");
            m_AndroidEnableTango           = FindPropertyAssert("AndroidEnableTango");

            SerializedProperty property = FindPropertyAssert("vrSettings");
            if (property != null)
                m_Enable360StereoCapture = property.FindPropertyRelative("enable360StereoCapture");

            m_VirtualTexturingSupportEnabled = FindPropertyAssert("virtualTexturingSupportEnabled");
            m_ShaderPrecisionModel = FindPropertyAssert("shaderPrecisionModel");

            m_SettingsExtensions = new ISettingEditorExtension[validPlatforms.Length];
            for (int i = 0; i < validPlatforms.Length; i++)
            {
                string module = ModuleManager.GetTargetStringFromBuildTargetGroup(validPlatforms[i].targetGroup);
                m_SettingsExtensions[i] = ModuleManager.GetEditorSettingsExtension(module);
                if (m_SettingsExtensions[i] != null)
                    m_SettingsExtensions[i].OnEnable(this);
            }

            for (int i = 0; i < m_SectionAnimators.Length; i++)
                m_SectionAnimators[i] = new AnimBool(m_SelectedSection.value == i);
            SetValueChangeListeners(Repaint);

            splashScreenEditor.OnEnable();
            iconsEditor.OnEnable();

            // we clear it just to be on the safe side:
            // we access this cache both from player settings editor and script side when changing api
            s_GraphicsDeviceLists.Clear();

            // Setup initial values to prevent immediate script recompile (or editor restart)
            BuildTargetGroup targetGroup = validPlatforms[selectedPlatform].targetGroup;
            serializedActiveInputHandler = m_ActiveInputHandler.intValue;
            serializedSuppressCommonWarnings = m_SuppressCommonWarnings.boolValue;
            serializedAllowUnsafeCode = m_AllowUnsafeCode.boolValue;
            serializedAdditionalCompilerArguments = GetAdditionalCompilerArgumentsForGroup(targetGroup);
            serializedScriptingDefines = GetScriptingDefineSymbolsForGroup(targetGroup);
            serializedAPICompatibilityLevel = GetApiCompatibilityLevelForTarget(targetGroup);
            serializedUseDeterministicCompilation = m_UseDeterministicCompilation.boolValue;

            InitReorderableScriptingDefineSymbolsList(targetGroup);
            InitReorderableAdditionalCompilerArgumentsList(targetGroup);
        }

        void OnDisable()
        {
            if (hasScriptingDefinesBeenModified)
            {
                if (EditorUtility.DisplayDialog("Scripting Define Symbols Have Been Modified", "Do you want to apply changes?", "Apply", "Revert"))
                {
                    SetScriptingDefineSymbolsForGroup(lastTargetGroup, scriptingDefinesList.ToArray());
                    RecompileScripts(RecompileReason.scriptingDefineSymbolsModified);
                }

                hasScriptingDefinesBeenModified = false;
            }

            if (hasAdditionalCompilerArgumentsBeenModified)
            {
                if (EditorUtility.DisplayDialog("Additional Compiler Arguments Have Been Modified", "Do you want to apply changes?", "Apply", "Revert"))
                {
                    SetAdditionalCompilerArgumentsForGroup(lastTargetGroup, additionalCompilerArgumentsList.ToArray());
                    RecompileScripts(RecompileReason.additionalCompilerArgumentsModified);
                }

                hasAdditionalCompilerArgumentsBeenModified = false;
            }

            // Ensure script compilation handling is returned to to EditorOnlyPlayerSettings
            if (!isPreset)
                PlayerSettings.isHandlingScriptRecompile = true;
        }

        public void SetValueChangeListeners(UnityAction action)
        {
            for (int i = 0; i < m_SectionAnimators.Length; i++)
            {
                m_SectionAnimators[i].valueChanged.RemoveAllListeners();
                m_SectionAnimators[i].valueChanged.AddListener(action);
            }

            m_ShowDefaultIsNativeResolution.valueChanged.RemoveAllListeners();
            m_ShowDefaultIsNativeResolution.valueChanged.AddListener(action);

            m_ShowResolution.valueChanged.RemoveAllListeners();
            m_ShowResolution.valueChanged.AddListener(action);
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        internal override string targetTitle
        {
            get
            {
                if (m_LocalizedTargetName == null)
                    m_LocalizedTargetName = L10n.Tr(target.name);
                return m_LocalizedTargetName;
            }
        }

        private void CheckUpdatePresetSelectorStatus()
        {
            if (isPreset)
                return;

            var selectors = Resources.FindObjectsOfTypeAll<PresetSelector>();
            var isOpen = selectors != null && selectors.Length > 0;
            hasPresetWindowClosed = (isPresetWindowOpen && !isOpen);
            isPresetWindowOpen = isOpen;

            if (isPresetWindowOpen)
                PlayerSettings.isHandlingScriptRecompile = false;
        }

        private void CheckConsistency(BuildTargetGroup targetGroup)
        {
            if (isPreset)
                return;

            // Scripting define symbols
            var currentDefines = GetScriptingDefineSymbolsForGroup(targetGroup);
            if (serializedScriptingDefines != currentDefines)
            {
                if (!hasScriptingDefinesBeenModified)
                {
                    serializedScriptingDefines = currentDefines;
                    UpdateScriptingDefineSymbolsLists();
                }

                if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                    scriptRecompileRequired = true;
            }

            // Additional compiler arguments
            var currentAdditionalCompilerArguments = GetAdditionalCompilerArgumentsForGroup(targetGroup);
            if (!serializedAdditionalCompilerArguments.SequenceEqual(currentAdditionalCompilerArguments))
            {
                if (!hasAdditionalCompilerArgumentsBeenModified)
                {
                    serializedAdditionalCompilerArguments = currentAdditionalCompilerArguments;
                    UpdateAdditionalCompilerArgumentsLists();
                }

                if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                    scriptRecompileRequired = true;
            }

            // API compatibility level
            var currentAPICompatibilityLevel = GetApiCompatibilityLevelForTarget(targetGroup);
            if (serializedAPICompatibilityLevel != currentAPICompatibilityLevel)
            {
                serializedAPICompatibilityLevel = currentAPICompatibilityLevel;
                if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                    scriptRecompileRequired = true;
            }

            // Use determinisitc compilation
            if (serializedUseDeterministicCompilation != m_UseDeterministicCompilation.boolValue)
            {
                serializedUseDeterministicCompilation = m_UseDeterministicCompilation.boolValue;
                scriptRecompileRequired = true;
            }

            // Allow unsafe code
            if (serializedAllowUnsafeCode != m_AllowUnsafeCode.boolValue)
            {
                serializedAllowUnsafeCode = m_AllowUnsafeCode.boolValue;
                scriptRecompileRequired = true;
            }

            // Stack trace log type
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            {
                var globalLogType = PlayerSettings.GetGlobalStackTraceLogType(logType);
                var serializedLogType = (StackTraceLogType)m_StackTraceTypes.GetArrayElementAtIndex((int)logType).intValue;

                if (serializedLogType != globalLogType)
                    PlayerSettings.SetGlobalStackTraceLogType(logType, serializedLogType);
            }
        }

        private void RecompileScripts(string reason)
        {
            if (isPreset)
                return;

            if (isPresetWindowOpen)
            {
                scriptRecompileRequired = true;
            }
            else
            {
                scriptRecompileRequired = false;
                PlayerSettings.RecompileScripts(reason);
            }
        }

        private void OnPresetSelectorClosed()
        {
            if (isPreset)
                return;

            if (scriptRecompileRequired)
            {
                PlayerSettings.RecompileScripts(RecompileReason.playerSettingsModified);
                scriptRecompileRequired = false;
            }

            PlayerSettings.isHandlingScriptRecompile = true;
        }

        public override void OnInspectorGUI()
        {
            var serializedObjectUpdated = serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.BeginVertical();
            {
                CommonSettings();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            int oldPlatform = selectedPlatform;
            selectedPlatform = EditorGUILayout.BeginPlatformGrouping(validPlatforms, null);
            if (EditorGUI.EndChangeCheck())
            {
                // Awesome hackery to get string from delayed textfield when switching platforms
                if (EditorGUI.s_DelayedTextEditor.IsEditingControl(scriptingDefinesControlID))
                {
                    EditorGUI.EndEditingActiveTextField();
                    GUIUtility.keyboardControl = 0;
                    string[] defines = PlayerSettings.ConvertScriptingDefineStringToArray(EditorGUI.s_DelayedTextEditor.text);
                    SetScriptingDefineSymbolsForGroup(validPlatforms[oldPlatform].targetGroup, defines);
                }
                // Reset focus when changing between platforms.
                // If we don't do this, the resolution width/height value will not update correctly when they have the focus
                GUI.FocusControl("");
            }

            BuildPlatform platform = validPlatforms[selectedPlatform];
            BuildTargetGroup targetGroup = platform.targetGroup;

            if (!isPreset)
            {
                CheckUpdatePresetSelectorStatus();
                CheckConsistency(targetGroup);
            }

            GUILayout.Label(string.Format(L10n.Tr("Settings for {0}"), validPlatforms[selectedPlatform].title.text));

            // Increase the offset to accomodate large labels, though keep a minimum of 150.
            EditorGUIUtility.labelWidth = Mathf.Max(150, EditorGUIUtility.labelWidth + 20);

            int sectionIndex = 0;

            if (serializedObjectUpdated)
            {
                m_IconsEditor.SerializedObjectUpdated();
                foreach (var settingsExtension in m_SettingsExtensions)
                {
                    settingsExtension?.SerializedObjectUpdated();
                }
            }

            m_IconsEditor.IconSectionGUI(targetGroup, m_SettingsExtensions[selectedPlatform], selectedPlatform, sectionIndex++);

            ResolutionSectionGUI(targetGroup, m_SettingsExtensions[selectedPlatform], sectionIndex++);
            m_SplashScreenEditor.SplashSectionGUI(platform, targetGroup, m_SettingsExtensions[selectedPlatform], sectionIndex++);
            DebugAndCrashReportingGUI(platform, targetGroup, m_SettingsExtensions[selectedPlatform], sectionIndex++);
            OtherSectionGUI(platform, targetGroup, m_SettingsExtensions[selectedPlatform], sectionIndex++);
            PublishSectionGUI(targetGroup, m_SettingsExtensions[selectedPlatform], sectionIndex++);

            if (sectionIndex != kNumberGUISections)
                Debug.LogError("Mismatched number of GUI sections.");

            EditorGUILayout.EndPlatformGrouping();

            serializedObject.ApplyModifiedProperties();

            if (hasPresetWindowClosed)
            {
                OnPresetSelectorClosed();
                hasPresetWindowClosed = false;
            }
        }

        private void CommonSettings()
        {
            EditorGUILayout.PropertyField(m_CompanyName);
            EditorGUILayout.PropertyField(m_ProductName);
            EditorGUILayout.PropertyField(m_ApplicationBundleVersion, EditorGUIUtility.TrTextContent("Version"));
            EditorGUILayout.Space();

            m_IconsEditor.LegacyIconSectionGUI();

            GUILayout.Space(3);

            Rect cursorPropertyRect = EditorGUILayout.GetControlRect(true, EditorGUI.kObjectFieldThumbnailHeight);
            EditorGUI.BeginProperty(cursorPropertyRect, SettingsContent.defaultCursor, m_DefaultCursor);
            m_DefaultCursor.objectReferenceValue = EditorGUI.ObjectField(cursorPropertyRect, SettingsContent.defaultCursor, m_DefaultCursor.objectReferenceValue, typeof(Texture2D), false);
            EditorGUI.EndProperty();

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, 0, SettingsContent.cursorHotspot);
            EditorGUI.PropertyField(rect, m_CursorHotspot, GUIContent.none);
        }

        public bool BeginSettingsBox(int nr, GUIContent header)
        {
            bool enabled = GUI.enabled;
            GUI.enabled = true; // we don't want to disable the expand behavior
            EditorGUILayout.BeginVertical(Styles.categoryBox);
            Rect r = GUILayoutUtility.GetRect(20, 21);
            EditorGUI.BeginChangeCheck();
            bool expanded = EditorGUI.FoldoutTitlebar(r, header, m_SelectedSection.value == nr, true, EditorStyles.inspectorTitlebarFlat, EditorStyles.inspectorTitlebarText);
            if (EditorGUI.EndChangeCheck())
            {
                m_SelectedSection.value = (expanded ? nr : -1);
                GUIUtility.keyboardControl = 0;
            }
            m_SectionAnimators[nr].target = expanded;
            GUI.enabled = enabled;
            EditorGUI.indentLevel++;
            return EditorGUILayout.BeginFadeGroup(m_SectionAnimators[nr].faded);
        }

        public void EndSettingsBox()
        {
            EditorGUILayout.EndFadeGroup();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        public void ShowSharedNote()
        {
            GUILayout.Label(SettingsContent.sharedBetweenPlatformsInfo, EditorStyles.miniLabel);
        }

        private static bool TargetSupportsOptionalBuiltinSplashScreen(BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension)
        {
            if (settingsExtension != null)
                return settingsExtension.CanShowUnitySplashScreen();

            return targetGroup == BuildTargetGroup.Standalone;
        }

        public void ResolutionSectionGUI(BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension, int sectionIndex = 0)
        {
            if (BeginSettingsBox(sectionIndex, SettingsContent.resolutionPresentationTitle))
            {
                // PLEASE DO NOT COPY SETTINGS TO APPEAR MULTIPLE PLACES IN THE CODE! See top of file for more info.

                {
                    // Resolution itself

                    if (settingsExtension != null)
                    {
                        float h = EditorGUI.kSingleLineHeight;
                        float kLabelFloatMinW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
                        float kLabelFloatMaxW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
                        settingsExtension.ResolutionSectionGUI(h, kLabelFloatMinW, kLabelFloatMaxW);
                    }

                    if (targetGroup == BuildTargetGroup.Standalone)
                    {
                        GUILayout.Label(SettingsContent.resolutionTitle, EditorStyles.boldLabel);

                        var fullscreenModes = new[] { FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen, FullScreenMode.MaximizedWindow, FullScreenMode.Windowed };
                        var fullscreenModeNames = new[] { SettingsContent.fullscreenWindow, SettingsContent.exclusiveFullscreen, SettingsContent.maximizedWindow, SettingsContent.windowed };
                        var fullscreenModeNew = FullScreenMode.FullScreenWindow;
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.PropertyScope(horizontal.rect, GUIContent.none, m_FullscreenMode))
                            {
                                fullscreenModeNew = BuildEnumPopup(m_FullscreenMode, SettingsContent.fullscreenMode, fullscreenModes, fullscreenModeNames);
                            }
                        }

                        bool defaultIsFullScreen = fullscreenModeNew != FullScreenMode.Windowed;
                        m_ShowDefaultIsNativeResolution.target = defaultIsFullScreen;
                        if (EditorGUILayout.BeginFadeGroup(m_ShowDefaultIsNativeResolution.faded))
                            EditorGUILayout.PropertyField(m_DefaultIsNativeResolution);
                        EditorGUILayout.EndFadeGroup();

                        m_ShowResolution.target = !(defaultIsFullScreen && m_DefaultIsNativeResolution.boolValue);
                        if (EditorGUILayout.BeginFadeGroup(m_ShowResolution.faded))
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(m_DefaultScreenWidth, SettingsContent.defaultScreenWidth);
                            if (EditorGUI.EndChangeCheck() && m_DefaultScreenWidth.intValue < 1)
                                m_DefaultScreenWidth.intValue = 1;

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(m_DefaultScreenHeight, SettingsContent.defaultScreenHeight);
                            if (EditorGUI.EndChangeCheck() && m_DefaultScreenHeight.intValue < 1)
                                m_DefaultScreenHeight.intValue = 1;
                        }
                        EditorGUILayout.EndFadeGroup();
                    }
                    if (targetGroup == BuildTargetGroup.Standalone)
                    {
                        EditorGUILayout.PropertyField(m_MacRetinaSupport, SettingsContent.macRetinaSupport);
                        EditorGUILayout.PropertyField(m_RunInBackground, SettingsContent.runInBackground);
                    }

                    if (settingsExtension != null && settingsExtension.SupportsOrientation())
                    {
                        GUILayout.Label(SettingsContent.orientationTitle, EditorStyles.boldLabel);

                        EditorGUILayout.PropertyField(m_DefaultScreenOrientation, SettingsContent.defaultScreenOrientation);

                        if (m_DefaultScreenOrientation.enumValueIndex == (int)UIOrientation.AutoRotation)
                        {
                            if (targetGroup == BuildTargetGroup.iOS)
                                EditorGUILayout.PropertyField(m_UseOSAutoRotation, SettingsContent.useOSAutoRotation);

                            EditorGUI.indentLevel++;

                            GUILayout.Label(SettingsContent.allowedOrientationTitle, EditorStyles.boldLabel);

                            bool somethingAllowed = m_AllowedAutoRotateToPortrait.boolValue
                                || m_AllowedAutoRotateToPortraitUpsideDown.boolValue
                                || m_AllowedAutoRotateToLandscapeRight.boolValue
                                || m_AllowedAutoRotateToLandscapeLeft.boolValue;

                            if (!somethingAllowed)
                            {
                                m_AllowedAutoRotateToPortrait.boolValue = true;
                                Debug.LogError("All orientations are disabled. Allowing portrait");
                            }

                            EditorGUILayout.PropertyField(m_AllowedAutoRotateToPortrait, SettingsContent.allowedAutoRotateToPortrait);
                            EditorGUILayout.PropertyField(m_AllowedAutoRotateToPortraitUpsideDown, SettingsContent.allowedAutoRotateToPortraitUpsideDown);
                            EditorGUILayout.PropertyField(m_AllowedAutoRotateToLandscapeRight, SettingsContent.allowedAutoRotateToLandscapeRight);
                            EditorGUILayout.PropertyField(m_AllowedAutoRotateToLandscapeLeft, SettingsContent.allowedAutoRotateToLandscapeLeft);

                            EditorGUI.indentLevel--;
                        }
                    }

                    if (targetGroup == BuildTargetGroup.iOS)
                    {
                        GUILayout.Label(SettingsContent.multitaskingSupportTitle, EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(m_UIRequiresFullScreen, SettingsContent.UIRequiresFullScreen);
                        EditorGUILayout.Space();

                        GUILayout.Label(SettingsContent.statusBarTitle, EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(m_UIStatusBarHidden, SettingsContent.UIStatusBarHidden);
                        EditorGUILayout.PropertyField(m_UIStatusBarStyle, SettingsContent.UIStatusBarStyle);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.Space();

                    // Standalone Player
                    if (targetGroup == BuildTargetGroup.Standalone)
                    {
                        GUILayout.Label(SettingsContent.standalonePlayerOptionsTitle, EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(m_CaptureSingleScreen);

                        EditorGUILayout.PropertyField(m_UsePlayerLog);
                        EditorGUILayout.PropertyField(m_ResizableWindow);

                        EditorGUILayout.PropertyField(m_VisibleInBackground, SettingsContent.visibleInBackground);

                        EditorGUILayout.PropertyField(m_AllowFullscreenSwitch, SettingsContent.allowFullscreenSwitch);

                        EditorGUILayout.PropertyField(m_ForceSingleInstance);
                        EditorGUILayout.PropertyField(m_UseFlipModelSwapchain, SettingsContent.useFlipModelSwapChain);

                        if (!PlayerSettings.useFlipModelSwapchain)
                        {
                            EditorGUILayout.HelpBox(SettingsContent.flipModelSwapChainWarning.text, MessageType.Warning, true);
                        }

                        if (isPreset)
                            EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(m_SupportedAspectRatios, true);

                        if (isPreset)
                            EditorGUI.indentLevel--;

                        EditorGUILayout.Space();
                    }

                    // integrated gpu color/depth bits setup
                    if (BuildTargetDiscovery.PlatformGroupHasFlag(targetGroup, TargetAttributes.HasIntegratedGPU))
                    {
                        // iOS, while supports 16bit FB through GL interface, use 32bit in hardware, so there is no need in 16bit
                        if (targetGroup != BuildTargetGroup.iOS &&
                            targetGroup != BuildTargetGroup.tvOS)
                        {
                            EditorGUILayout.PropertyField(m_Use32BitDisplayBuffer, SettingsContent.use32BitDisplayBuffer);
                        }

                        EditorGUILayout.PropertyField(m_DisableDepthAndStencilBuffers, SettingsContent.disableDepthAndStencilBuffers);
                        EditorGUILayout.PropertyField(m_PreserveFramebufferAlpha, SettingsContent.preserveFramebufferAlpha);
                    }
                    // activity indicator on loading
                    if (targetGroup == BuildTargetGroup.iOS)
                    {
                        EditorGUILayout.PropertyField(m_iosShowActivityIndicatorOnLoading, SettingsContent.iosShowActivityIndicatorOnLoading);
                    }
                    if (targetGroup == BuildTargetGroup.Android)
                    {
                        EditorGUILayout.PropertyField(m_androidShowActivityIndicatorOnLoading, SettingsContent.androidShowActivityIndicatorOnLoading);
                    }
                    if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.Android)
                    {
                        EditorGUILayout.Space();
                    }

                    ShowSharedNote();
                }
            }
            EndSettingsBox();
        }

        private void AddGraphicsDeviceMenuSelected(object userData, string[] options, int selected)
        {
            var target = (BuildTarget)userData;
            var apis = PlayerSettings.GetGraphicsAPIs(target);
            if (apis == null)
                return;
            var apiToAdd = (GraphicsDeviceType)Enum.Parse(typeof(GraphicsDeviceType), options[selected], true);
            var apiList = apis.ToList();
            apiList.Add(apiToAdd);
            apis = apiList.ToArray();
            PlayerSettings.SetGraphicsAPIs(target, apis);
        }

        private void AddGraphicsDeviceElement(BuildTarget target, Rect rect, ReorderableList list)
        {
            GraphicsDeviceType[] availableDevices = PlayerSettings.GetSupportedGraphicsAPIs(target);

            if (availableDevices == null || availableDevices.Length == 0)
                return;

            var names = new string[availableDevices.Length];
            var enabled = new bool[availableDevices.Length];
            for (int i = 0; i < availableDevices.Length; ++i)
            {
                names[i] = L10n.Tr(availableDevices[i].ToString());
                enabled[i] = !list.list.Contains(availableDevices[i]);
            }

            EditorUtility.DisplayCustomMenu(rect, names, enabled, null, AddGraphicsDeviceMenuSelected, target);
        }

        private bool CanRemoveGraphicsDeviceElement(ReorderableList list)
        {
            // don't allow removing the last API
            return list.list.Count >= 2;
        }

        private void RemoveGraphicsDeviceElement(BuildTarget target, ReorderableList list)
        {
            var apis = PlayerSettings.GetGraphicsAPIs(target);
            if (apis == null)
                return;
            // don't allow removing the last API
            if (apis.Length < 2)
            {
                EditorApplication.Beep();
                return;
            }

            var apiList = apis.ToList();
            apiList.RemoveAt(list.index);
            apis = apiList.ToArray();

            ApplyChangedGraphicsAPIList(target, apis, list.index == 0);
        }

        private void ReorderGraphicsDeviceElement(BuildTarget target, ReorderableList list)
        {
            var previousAPIs = PlayerSettings.GetGraphicsAPIs(target);
            var apiList = (List<GraphicsDeviceType>)list.list;
            var apis = apiList.ToArray();

            var firstAPIDifferent = (previousAPIs[0] != apis[0]);
            ApplyChangedGraphicsAPIList(target, apis, firstAPIDifferent);
        }

        // these two methods are needed for cases when you want to take some action depending on user choice
        // as changing graphics api will call GUIUtility.ExitGUI

        private struct ChangeGraphicsApiAction
        {
            public readonly bool changeList, reloadGfx;
            public ChangeGraphicsApiAction(bool doChange, bool doReload) { changeList = doChange; reloadGfx = doReload; }
        }
        private ChangeGraphicsApiAction CheckApplyGraphicsAPIList(BuildTarget target, bool firstEntryChanged)
        {
            bool doRestart = false;
            // If we're changing the first API for relevant editor, this will cause editor to switch: ask for scene save & confirmation
            if (firstEntryChanged && WillEditorUseFirstGraphicsAPI(target))
            {
                // If we have dirty scenes we need to save or discard changes before we restart editor.
                // Otherwise user will get a dialog later on where they can click cancel and put editor in a bad device state.
                var dirtyScenes = new List<Scene>();
                for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.isDirty)
                        dirtyScenes.Add(scene);
                }
                if (dirtyScenes.Count != 0)
                {
                    var result = EditorUtility.DisplayDialogComplex("Changing editor graphics API",
                        "You've changed the active graphics API. This requires a restart of the Editor. Do you want to save the Scene when restarting?",
                        "Save and Restart", "Cancel Changing API", "Discard Changes and Restart");
                    if (result == 1)
                    {
                        doRestart = false; // Cancel was selected
                    }
                    else
                    {
                        doRestart = true;
                        if (result == 0) // Save and Restart was selected
                        {
                            for (int i = 0; i < dirtyScenes.Count; ++i)
                            {
                                var saved = EditorSceneManager.SaveScene(dirtyScenes[i]);
                                if (saved == false)
                                {
                                    doRestart = false;
                                }
                            }
                        }
                        else // Discard Changes and Restart was selected
                        {
                            for (int i = 0; i < dirtyScenes.Count; ++i)
                                EditorSceneManager.ClearSceneDirtiness(dirtyScenes[i]);
                        }
                    }
                }
                else
                {
                    doRestart = EditorUtility.DisplayDialog("Changing editor graphics API",
                        "You've changed the active graphics API. This requires a restart of the Editor.",
                        "Restart Editor", "Not now");
                }
                return new ChangeGraphicsApiAction(doRestart, doRestart);
            }
            else
            {
                return new ChangeGraphicsApiAction(true, false);
            }
        }

        private void ApplyChangeGraphicsApiAction(BuildTarget target, GraphicsDeviceType[] apis, ChangeGraphicsApiAction action)
        {
            if (action.changeList)
                PlayerSettings.SetGraphicsAPIs(target, apis);
            else
                s_GraphicsDeviceLists.Remove(target); // we cancelled the list change, so remove the cached one

            if (action.reloadGfx)
            {
                EditorApplication.RequestCloseAndRelaunchWithCurrentArguments();
                GUIUtility.ExitGUI();
            }
        }

        private void ApplyChangedGraphicsAPIList(BuildTarget target, GraphicsDeviceType[] apis, bool firstEntryChanged)
        {
            ChangeGraphicsApiAction action = CheckApplyGraphicsAPIList(target, firstEntryChanged);
            ApplyChangeGraphicsApiAction(target, apis, action);
        }

        private void DrawGraphicsDeviceElement(BuildTarget target, Rect rect, int index, bool selected, bool focused)
        {
            var device = s_GraphicsDeviceLists[target].list[index];
            var name = device.ToString();
            if (name == "Direct3D12")
                name = "Direct3D12 (Experimental)";

            // For WebGL, display the actual WebGL version names instead of corresponding GLES APIs for clarification.
            if (target == BuildTarget.WebGL)
            {
                if (name == "OpenGLES3")
                    name = "WebGL 2.0";
                else if (name == "OpenGLES2")
                    name = "WebGL 1.0";
            }
            else if (target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
                if (name.Contains("OpenGLES"))
                    name += " (Deprecated)";
            }

            GUI.Label(rect, name, EditorStyles.label);
        }

        private static bool WillEditorUseFirstGraphicsAPI(BuildTarget targetPlatform)
        {
            return
                Application.platform == RuntimePlatform.WindowsEditor && targetPlatform == BuildTarget.StandaloneWindows ||
                Application.platform == RuntimePlatform.LinuxEditor && targetPlatform == BuildTarget.StandaloneLinux64 ||
                Application.platform == RuntimePlatform.OSXEditor && targetPlatform == BuildTarget.StandaloneOSX;
        }

        void OpenGLES31OptionsGUI(BuildTargetGroup targetGroup, BuildTarget targetPlatform)
        {
            // ES3.1 options only applicable on some platforms now
            var hasES31Options = (targetGroup == BuildTargetGroup.Android);
            if (!hasES31Options)
                return;

            var apis = PlayerSettings.GetGraphicsAPIs(targetPlatform);
            // only available if we include ES3, and not ES2
            var hasMinES3 = apis.Contains(GraphicsDeviceType.OpenGLES3) && !apis.Contains(GraphicsDeviceType.OpenGLES2);
            if (!hasMinES3)
                return;

            EditorGUILayout.PropertyField(m_RequireES31, SettingsContent.require31);
            EditorGUILayout.PropertyField(m_RequireES31AEP, SettingsContent.requireAEP);
            EditorGUILayout.PropertyField(m_RequireES32, SettingsContent.require32);
        }

        void GraphicsAPIsGUIOnePlatform(BuildTargetGroup targetGroup, BuildTarget targetPlatform, string platformTitle)
        {
            GraphicsDeviceType[] availableDevices = PlayerSettings.GetSupportedGraphicsAPIs(targetPlatform);
            // if no devices (e.g. no platform module), or we only have one possible choice, then no
            // point in having any UI
            if (availableDevices == null || availableDevices.Length < 2)
                return;

            // toggle for automatic API selection
            EditorGUI.BeginChangeCheck();
            var automatic = PlayerSettings.GetUseDefaultGraphicsAPIs(targetPlatform);
            automatic = EditorGUILayout.Toggle(string.Format(L10n.Tr("Auto Graphics API {0}"), (platformTitle ?? string.Empty)), automatic);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, SettingsContent.undoChangedGraphicsAPIString);
                PlayerSettings.SetUseDefaultGraphicsAPIs(targetPlatform, automatic);
            }

            // graphics API list if not automatic
            if (!automatic)
            {
                // note that editor will use first item, when we're in standalone settings
                if (WillEditorUseFirstGraphicsAPI(targetPlatform))
                {
                    EditorGUILayout.HelpBox(SettingsContent.recordingInfo.text, MessageType.Info, true);
                }

                var displayTitle = "Graphics APIs";
                if (platformTitle != null)
                    displayTitle += platformTitle;

                // create reorderable list for this target if needed
                if (!s_GraphicsDeviceLists.ContainsKey(targetPlatform))
                {
                    GraphicsDeviceType[] devices = PlayerSettings.GetGraphicsAPIs(targetPlatform);
                    var devicesList = (devices != null) ? devices.ToList() : new List<GraphicsDeviceType>();
                    var rlist = new ReorderableList(devicesList, typeof(GraphicsDeviceType), true, true, true, true);
                    rlist.onAddDropdownCallback = (rect, list) => AddGraphicsDeviceElement(targetPlatform, rect, list);
                    rlist.onCanRemoveCallback = CanRemoveGraphicsDeviceElement;
                    rlist.onRemoveCallback = (list) => RemoveGraphicsDeviceElement(targetPlatform, list);
                    rlist.onReorderCallback = (list) => ReorderGraphicsDeviceElement(targetPlatform, list);
                    rlist.drawElementCallback = (rect, index, isActive, isFocused) => DrawGraphicsDeviceElement(targetPlatform, rect, index, isActive, isFocused);
                    rlist.drawHeaderCallback = (rect) => GUI.Label(rect, displayTitle, EditorStyles.label);
                    rlist.elementHeight = 16;

                    s_GraphicsDeviceLists.Add(targetPlatform, rlist);
                }
                s_GraphicsDeviceLists[targetPlatform].DoLayoutList();

                // ES3.1 options
                OpenGLES31OptionsGUI(targetGroup, targetPlatform);

                //@TODO: undo
            }
        }

        void GraphicsAPIsGUI(BuildTargetGroup targetGroup, BuildTarget target)
        {
            // "standalone" is a generic group;
            // split it into win/mac/linux manually
            if (targetGroup == BuildTargetGroup.Standalone)
            {
                GraphicsAPIsGUIOnePlatform(targetGroup, BuildTarget.StandaloneWindows, " for Windows");
                GraphicsAPIsGUIOnePlatform(targetGroup, BuildTarget.StandaloneOSX, " for Mac");
                GraphicsAPIsGUIOnePlatform(targetGroup, BuildTarget.StandaloneLinux64, " for Linux");
            }
            else
            {
                GraphicsAPIsGUIOnePlatform(targetGroup, target, null);
            }
        }

        // Contains information about color gamuts supported by each platform.
        // If platform group is not in the dictionary, then it's assumed it supports only sRGB.
        // Color gamut player setting is not displayed for such platforms.
        //
        // This information might be useful for users that use the color gamut APIs,
        // we could expose it somehow
        private static Dictionary<BuildTargetGroup, List<ColorGamut>> s_SupportedColorGamuts =
            new Dictionary<BuildTargetGroup, List<ColorGamut>>
        {
            { BuildTargetGroup.Standalone, new List<ColorGamut> { ColorGamut.sRGB, ColorGamut.DisplayP3 } },
            { BuildTargetGroup.iOS, new List<ColorGamut> { ColorGamut.sRGB, ColorGamut.DisplayP3 } },
            { BuildTargetGroup.tvOS, new List<ColorGamut> { ColorGamut.sRGB, ColorGamut.DisplayP3 } },
            { BuildTargetGroup.Android, new List<ColorGamut> {ColorGamut.sRGB, ColorGamut.DisplayP3 } }
        };

        private static bool IsColorGamutSupportedOnTargetGroup(BuildTargetGroup targetGroup, ColorGamut gamut)
        {
            if (gamut == ColorGamut.sRGB)
                return true;
            if (s_SupportedColorGamuts.ContainsKey(targetGroup) &&
                s_SupportedColorGamuts[targetGroup].Contains(gamut))
                return true;
            return false;
        }

        private static string GetColorGamutDisplayString(BuildTargetGroup targetGroup, ColorGamut gamut)
        {
            string name = gamut.ToString();
            if (!IsColorGamutSupportedOnTargetGroup(targetGroup, gamut))
                name += L10n.Tr(" (not supported on this platform)");
            return name;
        }

        private void AddColorGamutElement(BuildTargetGroup targetGroup, Rect rect, ReorderableList list)
        {
            var availableColorGamuts = new ColorGamut[]
            {
                // Enable the gamuts when at least one platform supports them
                ColorGamut.sRGB,
                //ColorGamut.Rec709,
                //ColorGamut.Rec2020,
                ColorGamut.DisplayP3,
                //ColorGamut.HDR10,
                //ColorGamut.DolbyHDR
            };

            var names = new string[availableColorGamuts.Length];
            var enabled = new bool[availableColorGamuts.Length];
            for (int i = 0; i < availableColorGamuts.Length; ++i)
            {
                names[i] = GetColorGamutDisplayString(targetGroup, availableColorGamuts[i]);
                enabled[i] = !list.list.Contains(availableColorGamuts[i]);
            }

            EditorUtility.DisplayCustomMenu(rect, names, enabled, null, AddColorGamutMenuSelected, availableColorGamuts);
        }

        private void AddColorGamutMenuSelected(object userData, string[] options, int selected)
        {
            var colorGamuts = (ColorGamut[])userData;
            var colorGamutList = PlayerSettings.GetColorGamuts().ToList();
            colorGamutList.Add(colorGamuts[selected]);
            PlayerSettings.SetColorGamuts(colorGamutList.ToArray());
        }

        private bool CanRemoveColorGamutElement(ReorderableList list)
        {
            // don't allow removing the sRGB
            var colorGamutList = (List<ColorGamut>)list.list;
            return colorGamutList[list.index] != ColorGamut.sRGB;
        }

        private void RemoveColorGamutElement(ReorderableList list)
        {
            var colorGamutList = PlayerSettings.GetColorGamuts().ToList();
            // don't allow removing the last ColorGamut
            if (colorGamutList.Count < 2)
            {
                EditorApplication.Beep();
                return;
            }
            colorGamutList.RemoveAt(list.index);
            PlayerSettings.SetColorGamuts(colorGamutList.ToArray());
        }

        private void ReorderColorGamutElement(ReorderableList list)
        {
            var colorGamutList = (List<ColorGamut>)list.list;
            PlayerSettings.SetColorGamuts(colorGamutList.ToArray());
        }

        private void DrawColorGamutElement(BuildTargetGroup targetGroup, Rect rect, int index, bool selected, bool focused)
        {
            var colorGamut = s_ColorGamutList.list[index];
            GUI.Label(rect, GetColorGamutDisplayString(targetGroup, (ColorGamut)colorGamut), EditorStyles.label);
        }

        void ColorGamutGUI(BuildTargetGroup targetGroup)
        {
            if (!s_SupportedColorGamuts.ContainsKey(targetGroup))
                return;

            if (s_ColorGamutList == null)
            {
                ColorGamut[] colorGamuts = PlayerSettings.GetColorGamuts();
                var colorGamutsList = (colorGamuts != null) ? colorGamuts.ToList() : new List<ColorGamut>();
                var rlist = new ReorderableList(colorGamutsList, typeof(ColorGamut), true, true, true, true);
                rlist.onCanRemoveCallback = CanRemoveColorGamutElement;
                rlist.onRemoveCallback = RemoveColorGamutElement;
                rlist.onReorderCallback = ReorderColorGamutElement;
                rlist.elementHeight = 16;

                s_ColorGamutList = rlist;
            }

            // On standalone inspector mention that the setting applies only to Mac
            // (Temporarily until other standalones support this setting)
            GUIContent header = targetGroup == BuildTargetGroup.Standalone ? SettingsContent.colorGamutForMac : SettingsContent.colorGamut;
            s_ColorGamutList.drawHeaderCallback = (rect) =>
                GUI.Label(rect, header, EditorStyles.label);

            // we want to change the displayed text per platform, to indicate unsupported gamuts
            s_ColorGamutList.onAddDropdownCallback = (rect, list) =>
                AddColorGamutElement(targetGroup, rect, list);

            s_ColorGamutList.drawElementCallback = (rect, index, selected, focused) =>
                DrawColorGamutElement(targetGroup, rect, index, selected, focused);

            s_ColorGamutList.DoLayoutList();
        }

        public void DebugAndCrashReportingGUI(BuildPlatform platform, BuildTargetGroup targetGroup,
            ISettingEditorExtension settingsExtension, int sectionIndex = 3)
        {
            if (targetGroup != BuildTargetGroup.iOS && targetGroup != BuildTargetGroup.tvOS)
                return;

            if (BeginSettingsBox(sectionIndex, SettingsContent.debuggingCrashReportingTitle))
            {
                // PLEASE DO NOT COPY SETTINGS TO APPEAR MULTIPLE PLACES IN THE CODE! See top of file for more info.
                {
                    // Debugging
                    GUILayout.Label(SettingsContent.debuggingTitle, EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(m_EnableInternalProfiler, SettingsContent.enableInternalProfiler);
                    EditorGUILayout.Space();
                }

                {
                    // Crash reporting
                    GUILayout.Label(SettingsContent.crashReportingTitle, EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(m_ActionOnDotNetUnhandledException, SettingsContent.actionOnDotNetUnhandledException);
                    EditorGUILayout.PropertyField(m_LogObjCUncaughtExceptions, SettingsContent.logObjCUncaughtExceptions);

                    GUIContent crashReportApiContent = SettingsContent.enableCrashReportAPI;

                    bool apiFieldDisabled = false;

                    if (UnityEditor.CrashReporting.CrashReportingSettings.enabled)
                    {
                        // CrashReport API must be enabled if cloud crash reporting is enabled,
                        // so don't let them change the value of the checkbox
                        crashReportApiContent = new GUIContent(crashReportApiContent);  // Create a copy so we don't alter the style definition
                        apiFieldDisabled = true;
                        crashReportApiContent.tooltip = "CrashReport API must be enabled for Performance Reporting service.";
                        m_EnableCrashReportAPI.boolValue = true;
                    }

                    EditorGUI.BeginDisabledGroup(apiFieldDisabled);
                    EditorGUILayout.PropertyField(m_EnableCrashReportAPI, crashReportApiContent);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                }
            }
            EndSettingsBox();
        }

        public static void BuildDisabledEnumPopup(GUIContent selected, GUIContent uiString)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.Popup(EditorGUILayout.GetControlRect(true), uiString, 0, new GUIContent[] { selected });
            }
        }

        public static T BuildEnumPopup<T>(SerializedProperty prop, GUIContent uiString, T[] options, GUIContent[] optionNames)
        {
            T val = (T)(object)prop.intValue;
            T newVal = BuildEnumPopup(uiString, val, options, optionNames);

            // Update property if the popup value has changed
            if (!newVal.Equals(val))
            {
                prop.intValue = (int)(object)newVal;
                prop.serializedObject.ApplyModifiedProperties();
            }

            return newVal;
        }

        public static T BuildEnumPopup<T>(GUIContent uiString, T selected, T[] options, GUIContent[] optionNames)
        {
            // Display dropdown
            int idx = 0; // pick the first property when not found
            for (int i = 1; i < options.Length; ++i)
            {
                if (selected.Equals(options[i]))
                {
                    idx = i;
                    break;
                }
            }

            int newIdx = EditorGUILayout.Popup(uiString, idx, optionNames);
            return options[newIdx];
        }

        public void OtherSectionGUI(BuildPlatform platform, BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension, int sectionIndex = 4)
        {
            if (BeginSettingsBox(sectionIndex, SettingsContent.otherSettingsTitle))
            {
                // PLEASE DO NOT COPY SETTINGS TO APPEAR MULTIPLE PLACES IN THE CODE! See top of file for more info.
                OtherSectionRenderingGUI(platform, targetGroup, settingsExtension);
                OtherSectionVulkanSettingsGUI(targetGroup, settingsExtension);
                OtherSectionIdentificationGUI(targetGroup, settingsExtension);
                OtherSectionConfigurationGUI(platform, targetGroup, settingsExtension);
                OtherSectionScriptCompilationGUI(targetGroup);
                OtherSectionOptimizationGUI(platform, targetGroup);
                OtherSectionLoggingGUI();
                OtherSectionLegacyGUI(targetGroup);
                ShowSharedNote();
            }
            EndSettingsBox();
        }

        private void OtherSectionRenderingGUI(BuildPlatform platform, BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension)
        {
            // Rendering related settings
            GUILayout.Label(SettingsContent.renderingTitle, EditorStyles.boldLabel);

            // Color space (supported by all non deprecated platforms)
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying)) // switching color spaces in play mode is not supported
            {
                EditorGUI.BeginChangeCheck();
                int selectedValue = m_ActiveColorSpace.enumValueIndex;
                EditorGUILayout.PropertyField(m_ActiveColorSpace, SettingsContent.activeColorSpace);

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_ActiveColorSpace.enumValueIndex != selectedValue && EditorUtility.DisplayDialog("Changing Color Space", SettingsContent.changeColorSpaceString, $"Change to {(ColorSpace)m_ActiveColorSpace.enumValueIndex}", "Cancel"))
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                    else m_ActiveColorSpace.enumValueIndex = selectedValue;
                    GUIUtility.ExitGUI(); // Fixes case 690421
                }
            }

            // Display a warning for platforms that some devices don't support linear rendering if the settings are not fine for linear colorspace
            if (PlayerSettings.colorSpace == ColorSpace.Linear)
            {
                bool showWarning = false;
                GUIContent warningMessage = null;
                var apis = PlayerSettings.GetGraphicsAPIs(platform.defaultTarget);

                if (targetGroup == BuildTargetGroup.Android)
                {
                    // SRP should handle blits internally
                    bool hasBlitDisabled = (PlayerSettings.Android.blitType == AndroidBlitType.Never) && (GraphicsSettings.currentRenderPipeline == null);
                    showWarning = hasBlitDisabled || apis.Contains(GraphicsDeviceType.OpenGLES2);
                    warningMessage = SettingsContent.colorSpaceAndroidWarning;
                }
                else if (targetGroup == BuildTargetGroup.iOS || platform.targetGroup == BuildTargetGroup.tvOS)
                {
                    showWarning = apis.Contains(GraphicsDeviceType.OpenGLES3) || apis.Contains(GraphicsDeviceType.OpenGLES2);
                    warningMessage = SettingsContent.colorSpaceIOSWarning;
                }
                else if ((targetGroup == BuildTargetGroup.WebGL))
                {
                    showWarning = apis.Contains(GraphicsDeviceType.OpenGLES2);
                    warningMessage = SettingsContent.colorSpaceWebGLWarning;
                }

                if (showWarning)
                    EditorGUILayout.HelpBox(warningMessage.text, MessageType.Warning);
            }

            // Graphics APIs
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                GraphicsAPIsGUI(targetGroup, platform.defaultTarget);
            }

            // Output color spaces
            ColorGamutGUI(targetGroup);

            // Metal
            if (Application.platform == RuntimePlatform.OSXEditor && BuildTargetDiscovery.BuildTargetSupportsRenderer(platform, GraphicsDeviceType.Metal))
            {
                m_MetalAPIValidation.boolValue = EditorGUILayout.Toggle(SettingsContent.metalAPIValidation, m_MetalAPIValidation.boolValue);

                EditorGUILayout.PropertyField(m_MetalFramebufferOnly, SettingsContent.metalFramebufferOnly);
                if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS)
                    EditorGUILayout.PropertyField(m_MetalForceHardShadows, SettingsContent.metalForceHardShadows);

                int[] memorylessModeValues = { 0, 1, 2 };
                BuildEnumPopup(m_FramebufferDepthMemorylessMode, SettingsContent.framebufferDepthMemorylessMode, memorylessModeValues, SettingsContent.memorylessModeNames);
            }

            // Multithreaded rendering
            if (settingsExtension != null && settingsExtension.SupportsMultithreadedRendering())
                settingsExtension.MultithreadedRenderingGUI(targetGroup);

            // Batching section
            {
                int staticBatching, dynamicBatching;
                bool staticBatchingSupported = true;
                bool dynamicBatchingSupported = true;
                if (settingsExtension != null)
                {
                    staticBatchingSupported = settingsExtension.SupportsStaticBatching();
                    dynamicBatchingSupported = settingsExtension.SupportsDynamicBatching();
                }
                PlayerSettings.GetBatchingForPlatform(platform.defaultTarget, out staticBatching, out dynamicBatching);

                bool reset = false;
                if (staticBatchingSupported == false && staticBatching == 1)
                {
                    staticBatching = 0;
                    reset = true;
                }

                if (dynamicBatchingSupported == false && dynamicBatching == 1)
                {
                    dynamicBatching = 0;
                    reset = true;
                }

                if (reset)
                {
                    PlayerSettings.SetBatchingForPlatform(platform.defaultTarget, staticBatching, dynamicBatching);
                }

                EditorGUI.BeginChangeCheck();
                using (new EditorGUI.DisabledScope(!staticBatchingSupported))
                {
                    if (GUI.enabled)
                        staticBatching = EditorGUILayout.Toggle(SettingsContent.staticBatching, staticBatching != 0) ? 1 : 0;
                    else
                        EditorGUILayout.Toggle(SettingsContent.staticBatching, false);
                }

                if (GraphicsSettings.currentRenderPipeline == null)
                {
                    using (new EditorGUI.DisabledScope(!dynamicBatchingSupported))
                    {
                        dynamicBatching = EditorGUILayout.Toggle(SettingsContent.dynamicBatching, dynamicBatching != 0) ? 1 : 0;
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, SettingsContent.undoChangedBatchingString);
                    PlayerSettings.SetBatchingForPlatform(platform.defaultTarget, staticBatching, dynamicBatching);
                }
            }


            bool hdrDisplaySupported = false;
            bool gfxJobModesSupported = false;
            bool customLightmapEncodingSupported = (targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.WebGL);
            if (settingsExtension != null)
            {
                hdrDisplaySupported = settingsExtension.SupportsHighDynamicRangeDisplays();
                gfxJobModesSupported = settingsExtension.SupportsGfxJobModes();
                customLightmapEncodingSupported = customLightmapEncodingSupported || settingsExtension.SupportsCustomLightmapEncoding();
            }
            else
            {
                if (targetGroup == BuildTargetGroup.Standalone)
                {
                    GraphicsDeviceType[] gfxAPIs = PlayerSettings.GetGraphicsAPIs(platform.defaultTarget);

                    hdrDisplaySupported = gfxAPIs[0] == GraphicsDeviceType.Direct3D11 || gfxAPIs[0] == GraphicsDeviceType.Direct3D12 || gfxAPIs[0] == GraphicsDeviceType.Vulkan || gfxAPIs[0] == GraphicsDeviceType.Metal;
                }
            }

            // GPU Skinning toggle (only show on relevant platforms)
            if (!BuildTargetDiscovery.PlatformHasFlag(platform.defaultTarget, TargetAttributes.GPUSkinningNotSupported))
            {
                GraphicsDeviceType[] gfxAPIs = PlayerSettings.GetGraphicsAPIs(platform.defaultTarget);
                bool computeSkinningOnly =
                    gfxAPIs[0] != GraphicsDeviceType.XboxOneD3D12 &&
                    gfxAPIs[0] != GraphicsDeviceType.Direct3D11 &&
                    gfxAPIs[0] != GraphicsDeviceType.Direct3D12;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_SkinOnGPU, computeSkinningOnly ? SettingsContent.skinOnGPUCompute : SettingsContent.skinOnGPU);
                if (EditorGUI.EndChangeCheck())
                {
                    ShaderUtil.RecreateSkinnedMeshResources();
                }
            }

            bool graphicsJobsOptionEnabled = true;
            bool graphicsJobs = PlayerSettings.GetGraphicsJobsForPlatform(platform.defaultTarget);
            bool newGraphicsJobs = graphicsJobs;
            bool graphicsJobsModeOptionEnabled = graphicsJobs;

            if (targetGroup == BuildTargetGroup.XboxOne)
            {
                // on XBoxOne, we only have kGfxJobModeNative active for DX12 API and kGfxJobModeLegacy for the DX11 API
                // no need for a drop down popup for XBoxOne
                // also if XboxOneD3D12 is selected as GraphicsAPI, then we want to set graphics jobs and disable the user option
                GraphicsDeviceType[] gfxAPIs = PlayerSettings.GetGraphicsAPIs(platform.defaultTarget);
                GraphicsJobMode newGfxJobMode = GraphicsJobMode.Legacy;
                if (gfxAPIs[0] == GraphicsDeviceType.XboxOneD3D12)
                {
                    newGfxJobMode = GraphicsJobMode.Native;
                    graphicsJobsOptionEnabled = false;
                    if (graphicsJobs == false)
                    {
                        PlayerSettings.SetGraphicsJobsForPlatform(platform.defaultTarget, true);
                        graphicsJobs = true;
                        newGraphicsJobs = true;
                    }
                }
                PlayerSettings.SetGraphicsJobModeForPlatform(platform.defaultTarget, newGfxJobMode);
                graphicsJobsModeOptionEnabled = false;
            }
            EditorGUI.BeginChangeCheck();
            GUIContent graphicsJobsGUI = SettingsContent.graphicsJobsNonExperimental;
            switch (platform.defaultTarget)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                case BuildTarget.Android:
                    graphicsJobsGUI = SettingsContent.graphicsJobsExperimental;
                    break;
                default:
                    break;
            }

            using (new EditorGUI.DisabledScope(!graphicsJobsOptionEnabled))
            {
                if (GUI.enabled)
                {
                    newGraphicsJobs = EditorGUILayout.Toggle(graphicsJobsGUI, graphicsJobs);
                }
                else
                {
                    EditorGUILayout.Toggle(graphicsJobsGUI, graphicsJobs);
                }
            }
            if (EditorGUI.EndChangeCheck() && (newGraphicsJobs != graphicsJobs))
            {
                Undo.RecordObject(target, SettingsContent.undoChangedGraphicsJobsString);
                PlayerSettings.SetGraphicsJobsForPlatform(platform.defaultTarget, newGraphicsJobs);
            }

            if (gfxJobModesSupported)
            {
                EditorGUI.BeginChangeCheck();
                using (new EditorGUI.DisabledScope(!graphicsJobsModeOptionEnabled))
                {
                    GraphicsJobMode currGfxJobMode = PlayerSettings.GetGraphicsJobModeForPlatform(platform.defaultTarget);
                    GraphicsJobMode newGfxJobMode = BuildEnumPopup(SettingsContent.graphicsJobsMode, currGfxJobMode, m_GfxJobModeValues, m_GfxJobModeNames);
                    if (EditorGUI.EndChangeCheck() && (newGfxJobMode != currGfxJobMode))
                    {
                        Undo.RecordObject(target, SettingsContent.undoChangedGraphicsJobModeString);
                        PlayerSettings.SetGraphicsJobModeForPlatform(platform.defaultTarget, newGfxJobMode);
                    }
                }
            }

            if (settingsExtension != null && settingsExtension.SupportsCustomNormalMapEncoding())
            {
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || Lightmapping.isRunning))
                {
                    EditorGUI.BeginChangeCheck();
                    NormalMapEncoding oldEncoding = PlayerSettings.GetNormalMapEncoding(targetGroup);
                    NormalMapEncoding[] encodingValues = { NormalMapEncoding.XYZ, NormalMapEncoding.DXT5nm };
                    NormalMapEncoding newEncoding = BuildEnumPopup(SettingsContent.normalMapEncodingLabel, oldEncoding, encodingValues, SettingsContent.normalMapEncodingNames);
                    if (EditorGUI.EndChangeCheck() && newEncoding != oldEncoding)
                    {
                        PlayerSettings.SetNormalMapEncoding(targetGroup, newEncoding);
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                }
            }

            // Show Lightmap Encoding quality option
            if (customLightmapEncodingSupported)
            {
                using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || Lightmapping.isRunning))
                {
                    EditorGUI.BeginChangeCheck();
                    LightmapEncodingQuality encodingQuality = PlayerSettings.GetLightmapEncodingQualityForPlatformGroup(targetGroup);
                    LightmapEncodingQuality[] lightmapEncodingValues = { LightmapEncodingQuality.Low, LightmapEncodingQuality.Normal, LightmapEncodingQuality.High };
                    LightmapEncodingQuality newEncodingQuality = BuildEnumPopup(SettingsContent.lightmapEncodingLabel, encodingQuality, lightmapEncodingValues, SettingsContent.lightmapEncodingNames);
                    if (EditorGUI.EndChangeCheck() && encodingQuality != newEncodingQuality)
                    {
                        PlayerSettings.SetLightmapEncodingQualityForPlatformGroup(targetGroup, newEncodingQuality);

                        Lightmapping.OnUpdateLightmapEncoding(targetGroup);

                        serializedObject.ApplyModifiedProperties();

                        GUIUtility.ExitGUI();
                    }

                    if (encodingQuality == LightmapEncodingQuality.High)
                    {
                        if (targetGroup == BuildTargetGroup.WebGL)
                        {
                            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
                            if (apis.Contains(GraphicsDeviceType.OpenGLES2))
                            {
                                EditorGUILayout.HelpBox(SettingsContent.lightmapEncodingWebGLWarning.text, MessageType.Warning);
                            }
                        }
                    }

                    if (encodingQuality != LightmapEncodingQuality.Low)
                    {
                        if (targetGroup == BuildTargetGroup.iOS)
                        {
                            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS);
                            var hasMinAPI = apis.Contains(GraphicsDeviceType.Metal) && !apis.Contains(GraphicsDeviceType.OpenGLES3) && !apis.Contains(GraphicsDeviceType.OpenGLES2);
                            if (!hasMinAPI)
                                EditorGUILayout.HelpBox(SettingsContent.lightmapQualityIOSWarning.text, MessageType.Warning);
                        }
                        else if (targetGroup == BuildTargetGroup.tvOS)
                        {
                            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.tvOS);
                            var hasMinAPI = apis.Contains(GraphicsDeviceType.Metal) && !apis.Contains(GraphicsDeviceType.OpenGLES3) && !apis.Contains(GraphicsDeviceType.OpenGLES2);
                            if (!hasMinAPI)
                                EditorGUILayout.HelpBox(SettingsContent.lightmapQualityIOSWarning.text, MessageType.Warning);
                        }
                        else if (targetGroup == BuildTargetGroup.Android)
                        {
                            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                            var hasMinAPI = (apis.Contains(GraphicsDeviceType.Vulkan) || apis.Contains(GraphicsDeviceType.OpenGLES3)) && !apis.Contains(GraphicsDeviceType.OpenGLES2);
                            if (!hasMinAPI)
                                EditorGUILayout.HelpBox(SettingsContent.lightmapQualityAndroidWarning.text, MessageType.Warning);
                        }
                    }
                }
            }

            // Light map settings
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || Lightmapping.isRunning))
            {
                bool streamingEnabled = PlayerSettings.GetLightmapStreamingEnabledForPlatformGroup(targetGroup);
                int streamingPriority = PlayerSettings.GetLightmapStreamingPriorityForPlatformGroup(targetGroup);

                EditorGUI.BeginChangeCheck();
                streamingEnabled = EditorGUILayout.Toggle(SettingsContent.lightmapStreamingEnabled, streamingEnabled);
                if (streamingEnabled)
                {
                    EditorGUI.indentLevel++;
                    streamingPriority = EditorGUILayout.DelayedIntField(SettingsContent.lightmapStreamingPriority, streamingPriority);
                    EditorGUI.indentLevel--;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    PlayerSettings.SetLightmapStreamingEnabledForPlatformGroup(targetGroup, streamingEnabled);
                    PlayerSettings.SetLightmapStreamingPriorityForPlatformGroup(targetGroup, streamingPriority);

                    Lightmapping.OnUpdateLightmapStreaming(targetGroup);

                    serializedObject.ApplyModifiedProperties();

                    GUIUtility.ExitGUI();
                }
            }

            if (targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.WSA || targetGroup == BuildTargetGroup.WebGL || (settingsExtension != null && settingsExtension.SupportsFrameTimingStatistics()))
            {
                PlayerSettings.enableFrameTimingStats = EditorGUILayout.Toggle(SettingsContent.enableFrameTimingStats, PlayerSettings.enableFrameTimingStats);
                if (PlayerSettings.enableFrameTimingStats && targetGroup == BuildTargetGroup.WebGL)
                {
                    var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
                    if (apis.Contains(GraphicsDeviceType.OpenGLES2))
                    {
                        EditorGUILayout.HelpBox(SettingsContent.frameTimingStatsWebGLWarning.text, MessageType.Warning);
                    }
                }
            }

            if (hdrDisplaySupported)
            {
                string label = "Use display in HDR mode";
                string tooltip = "Switch the display to HDR output (on supported displays)" + ((targetGroup == BuildTargetGroup.XboxOne) ? " at start of application." : ".");
                bool oldUseHDRDisplay = PlayerSettings.useHDRDisplay;
                PlayerSettings.useHDRDisplay = EditorGUILayout.Toggle(EditorGUIUtility.TrTextContent(label, tooltip), oldUseHDRDisplay);
                bool requestRepaint = false;

                if (oldUseHDRDisplay != PlayerSettings.useHDRDisplay)
                    requestRepaint = true;

                if (targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.WSA)
                {
                    using (new EditorGUI.DisabledScope(!PlayerSettings.useHDRDisplay))
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            D3DHDRDisplayBitDepth oldBitDepth = PlayerSettings.D3DHDRBitDepth;
                            D3DHDRDisplayBitDepth[] bitDepthValues = { D3DHDRDisplayBitDepth.D3DHDRDisplayBitDepth10, D3DHDRDisplayBitDepth.D3DHDRDisplayBitDepth16 };
                            GUIContent HDRBitDepthLabel = EditorGUIUtility.TrTextContent("Swap Chain Bit Depth", "Affects the bit depth of the final swap chain format and color space.");
                            GUIContent[] HDRBitDepthNames = { EditorGUIUtility.TrTextContent("Bit Depth 10"), EditorGUIUtility.TrTextContent("Bit Depth 16") };

                            D3DHDRDisplayBitDepth bitDepth = BuildEnumPopup(HDRBitDepthLabel, oldBitDepth, bitDepthValues, HDRBitDepthNames);
                            if (EditorGUI.EndChangeCheck())
                            {
                                PlayerSettings.D3DHDRBitDepth = bitDepth;
                                if (oldBitDepth != bitDepth)
                                    requestRepaint = true;
                            }
                        }
                    }
                }

                if (requestRepaint)
                    EditorApplication.RequestRepaintAllViews();
            }

            // Virtual Texturing settings
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || EditorApplication.isCompiling))
            {
                EditorGUI.BeginChangeCheck();
                bool selectedValue = m_VirtualTexturingSupportEnabled.boolValue;
                m_VirtualTexturingSupportEnabled.boolValue = EditorGUILayout.Toggle(SettingsContent.virtualTexturingSupportEnabled, m_VirtualTexturingSupportEnabled.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (PlayerSettings.OnVirtualTexturingChanged())
                    {
                        PlayerSettings.SetVirtualTexturingSupportEnabled(m_VirtualTexturingSupportEnabled.boolValue);
                        EditorApplication.RequestCloseAndRelaunchWithCurrentArguments();
                        GUIUtility.ExitGUI();
                    }
                    else
                    {
                        m_VirtualTexturingSupportEnabled.boolValue = selectedValue;
                    }
                }
            }

            if (PlayerSettings.GetVirtualTexturingSupportEnabled())
            {
                // Test Platform compatibility
                bool platformSupportsVT = UnityEngine.Rendering.VirtualTexturingEditor.Building.IsPlatformSupportedForPlayer(platform.defaultTarget);
                if (!platformSupportsVT)
                {
                    EditorGUILayout.HelpBox(SettingsContent.virtualTexturingUnsupportedPlatformWarning.text, MessageType.Warning);
                }

                // Test for all three 'Automatic Graphics API for X' checkboxes and report API/Platform-specific error
                if (targetGroup == BuildTargetGroup.Standalone)
                {
                    if (VirtualTexturingInvalidGfxAPI(BuildTarget.StandaloneWindows, true) ||
                        VirtualTexturingInvalidGfxAPI(BuildTarget.StandaloneWindows64, true))
                    {
                        EditorGUILayout.HelpBox(SettingsContent.virtualTexturingUnsupportedAPIWin.text, MessageType.Warning);
                    }

                    if (VirtualTexturingInvalidGfxAPI(BuildTarget.StandaloneLinux64, true))
                    {
                        EditorGUILayout.HelpBox(SettingsContent.virtualTexturingUnsupportedAPILinux.text, MessageType.Warning);
                    }

                    if (VirtualTexturingInvalidGfxAPI(BuildTarget.StandaloneOSX, true))
                    {
                        EditorGUILayout.HelpBox(SettingsContent.virtualTexturingUnsupportedAPIMac.text, MessageType.Warning);
                    }
                }
                else
                {
                    if (platformSupportsVT && VirtualTexturingInvalidGfxAPI(platform.defaultTarget, false))
                    {
                        EditorGUILayout.HelpBox(SettingsContent.virtualTexturingUnsupportedAPI.text, MessageType.Warning);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || EditorApplication.isCompiling))
            {
                EditorGUI.BeginChangeCheck();

                ShaderPrecisionModel currShaderPrecisionModel = PlayerSettings.GetShaderPrecisionModel();
                ShaderPrecisionModel[] shaderPrecisionModelValues = { ShaderPrecisionModel.PlatformDefault, ShaderPrecisionModel.Unified };
                ShaderPrecisionModel newShaderPrecisionModel = BuildEnumPopup(SettingsContent.shaderPrecisionModel, currShaderPrecisionModel, shaderPrecisionModelValues, SettingsContent.shaderPrecisionModelOptions);
                if (EditorGUI.EndChangeCheck() && currShaderPrecisionModel != newShaderPrecisionModel)
                {
                    PlayerSettings.SetShaderPrecisionModel(newShaderPrecisionModel);
                }
            }

            EditorGUILayout.Space();

            Stereo360CaptureGUI(targetGroup);

            EditorGUILayout.Space();
        }

        private bool VirtualTexturingInvalidGfxAPI(BuildTarget target, bool checkEditor)
        {
            GraphicsDeviceType[] gfxTypes = PlayerSettings.GetGraphicsAPIs(target);

            bool supportedAPI = true;
            foreach (GraphicsDeviceType api in gfxTypes)
            {
                supportedAPI &= UnityEngine.Rendering.VirtualTexturingEditor.Building.IsRenderAPISupported(api, target, checkEditor);
            }

            return !supportedAPI;
        }

        private void OtherSectionIdentificationGUI(BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension)
        {
            // Identification

            if (settingsExtension != null && settingsExtension.HasIdentificationGUI())
            {
                GUILayout.Label(SettingsContent.identificationTitle, EditorStyles.boldLabel);
                settingsExtension.IdentificationSectionGUI();

                EditorGUILayout.Space();
            }
            else if (targetGroup == BuildTargetGroup.Standalone)
            {
                // TODO this should be move to an extension if we have one for MacOS or Standalone target at some point.
                GUILayout.Label(SettingsContent.macAppStoreTitle, EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(m_OverrideDefaultApplicationIdentifier, EditorGUIUtility.TrTextContent("Override Default Bundle Identifier"));

                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.PropertyScope(horizontal.rect, GUIContent.none, m_OverrideDefaultApplicationIdentifier))
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            ShowApplicationIdentifierUI(BuildTargetGroup.Standalone, "Bundle Identifier", "'CFBundleIdentifier'");
                        }
                    }
                }

                PlayerSettingsEditor.ShowBuildNumberUI(m_BuildNumber, BuildTargetGroup.Standalone, "Build", "'CFBundleVersion'");

                EditorGUILayout.PropertyField(m_MacAppStoreCategory, SettingsContent.macAppStoreCategory);
                EditorGUILayout.PropertyField(m_UseMacAppStoreValidation, SettingsContent.useMacAppStoreValidation);

                EditorGUILayout.Space();
            }
        }

        private void OtherSectionVulkanSettingsGUI(BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension)
        {
            // Standalone targets don't have a settingsExtension but support vulkan
            if (settingsExtension != null && !settingsExtension.ShouldShowVulkanSettings())
                return;

            GUILayout.Label(SettingsContent.vulkanSettingsTitle, EditorStyles.boldLabel);

            PlayerSettings.vulkanEnableSetSRGBWrite = EditorGUILayout.Toggle(SettingsContent.vulkanEnableSetSRGBWrite, PlayerSettings.vulkanEnableSetSRGBWrite);
            EditorGUILayout.PropertyField(m_VulkanNumSwapchainBuffers, SettingsContent.vulkanNumSwapchainBuffers);
            PlayerSettings.vulkanNumSwapchainBuffers = (UInt32)m_VulkanNumSwapchainBuffers.intValue;
            EditorGUILayout.PropertyField(m_VulkanEnableLateAcquireNextImage, SettingsContent.vulkanEnableLateAcquireNextImage);
            EditorGUILayout.PropertyField(m_VulkanEnableCommandBufferRecycling, SettingsContent.vulkanEnableCommandBufferRecycling);

            if (settingsExtension != null && settingsExtension.ShouldShowVulkanSettings())
                settingsExtension.VulkanSectionGUI();

            EditorGUILayout.Space();
        }

        internal void ShowPlatformIconsByKind(PlatformIconFieldGroup iconFieldGroup, bool foldByKind = true, bool foldBySubkind = true)
        {
            m_IconsEditor.ShowPlatformIconsByKind(iconFieldGroup, foldByKind, foldBySubkind);
        }

        internal static GUIContent GetApplicationIdentifierError(BuildTargetGroup targetGroup)
        {
            if (targetGroup == BuildTargetGroup.Android)
                return SettingsContent.packageNameError;

            return SettingsContent.applicationIdentifierError;
        }

        internal void ShowApplicationIdentifierUI(BuildTargetGroup targetGroup, string label, string tooltip)
        {
            var overrideDefaultID = m_OverrideDefaultApplicationIdentifier.boolValue;
            var defaultIdentifier = String.Format("com.{0}.{1}", m_CompanyName.stringValue, m_ProductName.stringValue);
            var oldIdentifier = "";
            var currentIdentifier = PlayerSettings.SanitizeApplicationIdentifier(defaultIdentifier, targetGroup);
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroupName(targetGroup);
            var warningMessage = SettingsContent.applicationIdentifierWarning.text;
            var errorMessage = GetApplicationIdentifierError(targetGroup).text;

            string GetSanitizedApplicationIdentifier()
            {
                var sanitizedIdentifier = PlayerSettings.SanitizeApplicationIdentifier(currentIdentifier, targetGroup);

                if (currentIdentifier != oldIdentifier) {
                    if (!overrideDefaultID && !PlayerSettings.IsApplicationIdentifierValid(currentIdentifier, targetGroup))
                        Debug.LogError(errorMessage);
                    else if (overrideDefaultID && sanitizedIdentifier != currentIdentifier)
                        Debug.LogWarning(warningMessage);
                }

                return sanitizedIdentifier;
            }

            if (!m_ApplicationIdentifier.serializedObject.isEditingMultipleObjects)
            {
                m_ApplicationIdentifier.TryGetMapEntry(buildTargetGroup, out var entry);

                if (entry != null)
                    oldIdentifier = entry.FindPropertyRelative("second").stringValue;

                if (currentIdentifier != oldIdentifier)
                {
                    if (overrideDefaultID)
                        currentIdentifier = oldIdentifier;
                    else
                        m_ApplicationIdentifier.SetMapValue(buildTargetGroup, currentIdentifier);
                }

                EditorGUILayout.BeginVertical();
                EditorGUI.BeginChangeCheck();

                using (new EditorGUI.DisabledScope(!overrideDefaultID))
                {
                    currentIdentifier = GetSanitizedApplicationIdentifier();
                    currentIdentifier = EditorGUILayout.TextField(EditorGUIUtility.TrTextContent(label, tooltip), currentIdentifier);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    currentIdentifier = GetSanitizedApplicationIdentifier();
                    m_ApplicationIdentifier.SetMapValue(buildTargetGroup, currentIdentifier);
                }

                if (currentIdentifier == "com.Company.ProductName" || currentIdentifier == "com.unity3d.player")
                    EditorGUILayout.HelpBox("Don't forget to set the Application Identifier.", MessageType.Warning);
                else if (!PlayerSettings.IsApplicationIdentifierValid(currentIdentifier, targetGroup))
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                else if (!overrideDefaultID && currentIdentifier != defaultIdentifier)
                    EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);

                EditorGUILayout.EndVertical();
            }
        }

        internal static void ShowBuildNumberUI(SerializedProperty prop, BuildTargetGroup targetGroup, string label, string tooltip)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroupName(targetGroup);

            if (!prop.serializedObject.isEditingMultipleObjects)
            {
                prop.TryGetMapEntry(buildTargetGroup, out var entry);

                if (entry != null)
                {
                    var buildNumber = entry.FindPropertyRelative("second");
                    EditorGUILayout.PropertyField(buildNumber, EditorGUIUtility.TrTextContent(label, tooltip));
                }
            }
        }

        private bool ShouldRestartEditorToApplySetting()
        {
            return EditorUtility.DisplayDialog("Unity editor restart required", "The Unity editor must be restarted for this change to take effect.  Cancel to revert changes.", "Apply", "Cancel");
        }

        private ScriptingImplementation GetCurrentBackendForTarget(BuildTargetGroup targetGroup)
        {
            if (m_ScriptingBackend.TryGetMapEntry(BuildPipeline.GetBuildTargetGroupName(targetGroup), out var entry))
                return (ScriptingImplementation)entry.FindPropertyRelative("second").intValue;
            else
                return PlayerSettings.GetDefaultScriptingBackend(targetGroup);
        }

        private Il2CppCompilerConfiguration GetCurrentIl2CppCompilerConfigurationForTarget(BuildTargetGroup targetGroup)
        {
            if (m_Il2CppCompilerConfiguration.TryGetMapEntry(BuildPipeline.GetBuildTargetGroupName(targetGroup), out var entry))
                return (Il2CppCompilerConfiguration)entry.FindPropertyRelative("second").intValue;
            else
                return Il2CppCompilerConfiguration.Release;
        }

        private ManagedStrippingLevel GetCurrentManagedStrippingLevelForTarget(BuildTargetGroup targetGroup, ScriptingImplementation backend)
        {
            if (m_ManagedStrippingLevel.TryGetMapEntry(BuildPipeline.GetBuildTargetGroupName(targetGroup), out var entry))
                return (ManagedStrippingLevel)entry.FindPropertyRelative("second").intValue;
            else
            {
                if (backend == ScriptingImplementation.IL2CPP)
                    return ManagedStrippingLevel.Low;
                else
                    return ManagedStrippingLevel.Disabled;
            }
        }

        private ApiCompatibilityLevel GetApiCompatibilityLevelForTarget(BuildTargetGroup targetGroup)
        {
            if (m_APICompatibilityLevel.TryGetMapEntry(BuildPipeline.GetBuildTargetGroupName(targetGroup), out var entry))
                return (ApiCompatibilityLevel)entry.FindPropertyRelative("second").intValue;
            else
                // See comment in EditorOnlyPlayerSettings regarding defaultApiCompatibilityLevel
                return (ApiCompatibilityLevel)m_DefaultAPICompatibilityLevel.intValue;
        }

        private void OtherSectionConfigurationGUI(BuildPlatform platform, BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension)
        {
            // Configuration
            GUILayout.Label(SettingsContent.configurationTitle, EditorStyles.boldLabel);

            // scripting runtime settings in play mode are not supported
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                // Scripting back-end
                bool allowCompilerConfigurationSelection = false;
                ScriptingImplementation currentBackend = GetCurrentBackendForTarget(targetGroup);
                using (new EditorGUI.DisabledScope(m_SerializedObject.isEditingMultipleObjects))
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        using (var propertyScope = new EditorGUI.PropertyScope(horizontal.rect, GUIContent.none, m_ScriptingBackend))
                        {
                            IScriptingImplementations scripting = ModuleManager.GetScriptingImplementations(targetGroup);

                            if (scripting == null)
                            {
                                allowCompilerConfigurationSelection = true; // All platforms that support only one scripting backend are IL2CPP platforms
                                BuildDisabledEnumPopup(SettingsContent.scriptingDefault, SettingsContent.scriptingBackend);
                            }
                            else
                            {
                                var backends = scripting.Enabled();

                                allowCompilerConfigurationSelection = currentBackend == ScriptingImplementation.IL2CPP && scripting.AllowIL2CPPCompilerConfigurationSelection();
                                ScriptingImplementation newBackend;

                                if (backends.Length == 1)
                                {
                                    newBackend = backends[0];
                                    BuildDisabledEnumPopup(GetNiceScriptingBackendName(backends[0]), SettingsContent.scriptingBackend);
                                }
                                else
                                {
                                    newBackend = BuildEnumPopup(SettingsContent.scriptingBackend, currentBackend, backends, GetNiceScriptingBackendNames(backends));
                                }

                                if (newBackend != currentBackend)
                                {
                                    m_ScriptingBackend.SetMapValue(BuildPipeline.GetBuildTargetGroupName(targetGroup), (int)newBackend);
                                    currentBackend = newBackend;
                                }
                            }
                        }
                    }
                }

                // Api Compatibility Level
                using (new EditorGUI.DisabledScope(m_SerializedObject.isEditingMultipleObjects))
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        using (var propertyScope = new EditorGUI.PropertyScope(horizontal.rect, GUIContent.none, m_APICompatibilityLevel))
                        {
                            var currentAPICompatibilityLevel = GetApiCompatibilityLevelForTarget(targetGroup);
                            var availableCompatibilityLevels = new ApiCompatibilityLevel[] { ApiCompatibilityLevel.NET_4_6, ApiCompatibilityLevel.NET_Standard_2_0 };
                            currentAPICompatibilityLevel = BuildEnumPopup(
                                SettingsContent.apiCompatibilityLevel,
                                currentAPICompatibilityLevel,
                                availableCompatibilityLevels,
                                GetNiceApiCompatibilityLevelNames(availableCompatibilityLevels)
                            );

                            if (serializedAPICompatibilityLevel != currentAPICompatibilityLevel)
                            {
                                m_APICompatibilityLevel.SetMapValue(BuildPipeline.GetBuildTargetGroupName(targetGroup), (int)currentAPICompatibilityLevel);
                                serializedAPICompatibilityLevel = currentAPICompatibilityLevel;

                                if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                                    RecompileScripts(RecompileReason.apiCompatibilityLevelModified);
                            }
                        }
                    }
                }

                // Il2cpp Compiler Configuration
                using (new EditorGUI.DisabledScope(m_SerializedObject.isEditingMultipleObjects))
                {
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        using (var propertyScope = new EditorGUI.PropertyScope(horizontal.rect, GUIContent.none, m_Il2CppCompilerConfiguration))
                        {
                            using (new EditorGUI.DisabledScope(!allowCompilerConfigurationSelection))
                            {
                                Il2CppCompilerConfiguration currentConfiguration = GetCurrentIl2CppCompilerConfigurationForTarget(targetGroup);

                                var configurations = GetIl2CppCompilerConfigurations();
                                var configurationNames = GetIl2CppCompilerConfigurationNames();

                                var newConfiguration = BuildEnumPopup(SettingsContent.il2cppCompilerConfiguration, currentConfiguration, configurations, configurationNames);

                                if (currentConfiguration != newConfiguration)
                                    m_Il2CppCompilerConfiguration.SetMapValue(BuildPipeline.GetBuildTargetGroupName(targetGroup), (int)newConfiguration);
                            }
                        }
                    }
                }

                bool gcIncrementalEnabled = BuildPipeline.IsFeatureSupported("ENABLE_SCRIPTING_GC_WBARRIERS", platform.defaultTarget);
                if (targetGroup == BuildTargetGroup.iOS)
                    gcIncrementalEnabled = gcIncrementalEnabled && currentBackend == ScriptingImplementation.IL2CPP;

                using (new EditorGUI.DisabledScope(!gcIncrementalEnabled))
                {
                    var oldValue = m_GCIncremental.boolValue;
                    EditorGUILayout.PropertyField(m_GCIncremental, SettingsContent.gcIncremental);
                    if (m_GCIncremental.boolValue != oldValue)
                    {
                        // Give the user a chance to change mind and revert changes.
                        if (ShouldRestartEditorToApplySetting())
                        {
                            m_GCIncremental.serializedObject.ApplyModifiedProperties();
                            EditorApplication.OpenProject(Environment.CurrentDirectory);
                        }
                        else
                            m_GCIncremental.boolValue = oldValue;
                    }
                }

                EditorGUILayout.PropertyField(m_AssemblyVersionValidation,
                    (PlayerSettings.GetScriptingBackend(targetGroup) != ScriptingImplementation.Mono2x) ? SettingsContent.assemblyVersionValidationEditorOnly : SettingsContent.assemblyVersionValidation);
            }

            // Privacy permissions
            bool showPrivacyPermissions =
                targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS ||
                targetGroup == BuildTargetGroup.Standalone;

            if (showPrivacyPermissions)
            {
                EditorGUILayout.PropertyField(m_CameraUsageDescription, SettingsContent.cameraUsageDescription);
                EditorGUILayout.PropertyField(m_MicrophoneUsageDescription, SettingsContent.microphoneUsageDescription);

                if (targetGroup == BuildTargetGroup.Standalone)
                    EditorGUILayout.PropertyField(m_BluetoothUsageDescription, SettingsContent.bluetoothUsageDescription);

                if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS)
                    EditorGUILayout.PropertyField(m_LocationUsageDescription, SettingsContent.locationUsageDescription);
            }

            bool showMobileSection =
                targetGroup == BuildTargetGroup.iOS ||
                targetGroup == BuildTargetGroup.tvOS ||
                targetGroup == BuildTargetGroup.Android ||
                targetGroup == BuildTargetGroup.WSA;

            // mobile-only settings
            if (showMobileSection)
            {
                if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS)
                    EditorGUILayout.PropertyField(m_useOnDemandResources, SettingsContent.useOnDemandResources);

                bool supportsAccelerometerFrequency =
                    targetGroup == BuildTargetGroup.iOS ||
                    targetGroup == BuildTargetGroup.tvOS ||
                    targetGroup == BuildTargetGroup.WSA;
                if (supportsAccelerometerFrequency)
                    EditorGUILayout.PropertyField(m_AccelerometerFrequency, SettingsContent.accelerometerFrequency);

                if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS || targetGroup == BuildTargetGroup.Android)
                {
                    EditorGUILayout.PropertyField(m_MuteOtherAudioSources, SettingsContent.muteOtherAudioSources);

                    if (m_MuteOtherAudioSources.boolValue == false && targetGroup == BuildTargetGroup.iOS)
                        EditorGUILayout.HelpBox(SettingsContent.iOSExternalAudioInputNotSupported.text, MessageType.Warning);
                }

                // TVOS TODO: check what should stay or go
                if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS)
                {
                    if (targetGroup == BuildTargetGroup.iOS)
                    {
                        EditorGUILayout.PropertyField(m_PrepareIOSForRecording, SettingsContent.prepareIOSForRecording);
                        EditorGUILayout.PropertyField(m_ForceIOSSpeakersWhenRecording, SettingsContent.forceIOSSpeakersWhenRecording);
                    }
                    EditorGUILayout.PropertyField(m_UIRequiresPersistentWiFi, SettingsContent.UIRequiresPersistentWiFi);
                    EditorGUILayout.PropertyField(m_IOSAllowHTTPDownload, SettingsContent.iOSAllowHTTPDownload);
                    EditorGUILayout.PropertyField(m_IOSURLSchemes, SettingsContent.iOSURLSchemes, true);
                }
            }

            if (settingsExtension != null)
                settingsExtension.ConfigurationSectionGUI();

            // Active input handling
            using (var vertical = new EditorGUILayout.VerticalScope())
            {
                var currValue = m_ActiveInputHandler.intValue;

                using (var propertyScope = new EditorGUI.PropertyScope(vertical.rect, GUIContent.none, m_ActiveInputHandler))
                {
                    m_ActiveInputHandler.intValue = EditorGUILayout.Popup(SettingsContent.activeInputHandling, m_ActiveInputHandler.intValue, SettingsContent.activeInputHandlingOptions);
                }

                if (m_ActiveInputHandler.intValue != currValue)
                {
                    // Give the user a chance to change mind and revert changes.
                    if (ShouldRestartEditorToApplySetting())
                    {
                        m_ActiveInputHandler.serializedObject.ApplyModifiedProperties();
                        EditorApplication.RestartEditorAndRecompileScripts();
                    }
                    else
                        m_ActiveInputHandler.intValue = currValue;
                }
            }

            EditorGUILayout.Space();
        }

        private string GetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup)
        {
            string defines = string.Empty;
            if (m_ScriptingDefines.TryGetMapEntry((int)targetGroup, out var entry))
            {
                defines = entry.FindPropertyRelative("second").stringValue;
            }
            return defines;
        }

        private void SetScriptingDefineSymbolsForGroup(BuildTargetGroup targetGroup, string[] defines)
        {
            m_ScriptingDefines.SetMapValue((int)targetGroup, PlayerSettings.ConvertScriptingDefineArrayToString(defines));
        }

        string[] GetAdditionalCompilerArgumentsForGroup(BuildTargetGroup targetGroup)
        {
            if (m_AdditionalCompilerArguments.TryGetMapEntry((int)targetGroup, out var entry))
            {
                var serializedArguments = entry.FindPropertyRelative("second");
                var arguments = new string[serializedArguments.arraySize];

                for (int i = 0; i < serializedArguments.arraySize; ++i)
                {
                    arguments[i] = serializedArguments.GetArrayElementAtIndex(i).stringValue;
                }

                return arguments;
            }

            return new string[0];
        }

        void SetAdditionalCompilerArgumentsForGroup(BuildTargetGroup targetGroup, string[] arguments)
        {
            m_AdditionalCompilerArguments.SetMapValue((int)targetGroup, arguments);
        }

        private void OtherSectionScriptCompilationGUI(BuildTargetGroup targetGroup)
        {
            // Configuration
            GUILayout.Label(SettingsContent.scriptCompilationTitle, EditorStyles.boldLabel);

            // User script defines
            using (new EditorGUI.DisabledScope(m_SerializedObject.isEditingMultipleObjects))
            {
                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    lastTargetGroup = targetGroup;

                    if (serializedScriptingDefines == null || scriptingDefineSymbolsList == null)
                        InitReorderableScriptingDefineSymbolsList(targetGroup);

                    scriptingDefineSymbolsList.DoLayoutList();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        var GUIState = GUI.enabled;

                        if (GUILayout.Button(SettingsContent.scriptingDefineSymbolsCopyDefines,
                            EditorStyles.miniButton))
                        {
                            EditorGUIUtility.systemCopyBuffer = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                        }

                        GUI.enabled = hasScriptingDefinesBeenModified;

                        if (GUILayout.Button(SettingsContent.scriptingDefineSymbolsApplyRevert, EditorStyles.miniButton))
                        {
                            UpdateScriptingDefineSymbolsLists();
                        }

                        if (GUILayout.Button(SettingsContent.scriptingDefineSymbolsApply, EditorStyles.miniButton))
                        {
                            // Make sure to remove focus from reorderable list text field on apply
                            GUI.FocusControl(null);

                            SetScriptingDefineSymbolsForGroup(targetGroup, scriptingDefinesList.ToArray());

                            // Get Scripting Define Symbols without duplicates
                            serializedScriptingDefines = GetScriptingDefineSymbolsForGroup(targetGroup);
                            UpdateScriptingDefineSymbolsLists();

                            if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                                RecompileScripts(RecompileReason.scriptingDefineSymbolsModified);
                        }

                        // Set previous GUIState
                        GUI.enabled = GUIState;
                    }

                    scriptingDefinesControlID = EditorGUIUtility.s_LastControlID;
                }

                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    if (serializedAdditionalCompilerArguments == null || additionalCompilerArgumentsReorderableList == null)
                    {
                        InitReorderableAdditionalCompilerArgumentsList(targetGroup);
                    }

                    using (new EditorGUI.PropertyScope(vertical.rect, GUIContent.none, m_AdditionalCompilerArguments))
                    {
                        additionalCompilerArgumentsReorderableList.DoLayoutList();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();

                            using (new EditorGUI.DisabledScope(!hasAdditionalCompilerArgumentsBeenModified))
                            {
                                if (GUILayout.Button(SettingsContent.scriptingDefineSymbolsApplyRevert, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                                {
                                    UpdateAdditionalCompilerArgumentsLists();
                                }

                                if (GUILayout.Button(SettingsContent.scriptingDefineSymbolsApply, EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                                {
                                    SetAdditionalCompilerArgumentsForGroup(targetGroup, additionalCompilerArgumentsList.ToArray());

                                    // Get Additional Compiler Arguments without duplicates
                                    serializedAdditionalCompilerArguments = GetAdditionalCompilerArgumentsForGroup(targetGroup);
                                    UpdateAdditionalCompilerArgumentsLists();

                                    if (EditorUserBuildSettings.activeBuildTargetGroup == targetGroup)
                                    {
                                        RecompileScripts(RecompileReason.additionalCompilerArgumentsModified);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Suppress common warnings
            EditorGUILayout.PropertyField(m_SuppressCommonWarnings, SettingsContent.suppressCommonWarnings);
            if (serializedSuppressCommonWarnings != m_SuppressCommonWarnings.boolValue)
            {
                serializedSuppressCommonWarnings = m_SuppressCommonWarnings.boolValue;
                RecompileScripts(RecompileReason.suppressCommonWarningsModified);
            }

            // Allow unsafe code
            EditorGUILayout.PropertyField(m_AllowUnsafeCode, SettingsContent.allowUnsafeCode);
            if (serializedAllowUnsafeCode != m_AllowUnsafeCode.boolValue)
            {
                serializedAllowUnsafeCode = m_AllowUnsafeCode.boolValue;
                RecompileScripts(RecompileReason.allowUnsafeCodeModified);
            }

            // Use deterministic compliation
            EditorGUILayout.PropertyField(m_UseDeterministicCompilation, SettingsContent.useDeterministicCompilation);
            if (serializedUseDeterministicCompilation != m_UseDeterministicCompilation.boolValue)
            {
                serializedUseDeterministicCompilation = m_UseDeterministicCompilation.boolValue;
                RecompileScripts(RecompileReason.useDeterministicCompilationModified);
            }

            EditorGUILayout.PropertyField(m_EnableRoslynAnalyzers, SettingsContent.enableRoslynAnalyzers);
            EditorGUILayout.PropertyField(m_UseReferenceAssemblies, SettingsContent.useReferenceAssembclies);
        }

        void DrawTextField(Rect rect, int index)
        {
            // Handle list selection before the TextField grabs input
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                if (scriptingDefineSymbolsList.index != index)
                {
                    scriptingDefineSymbolsList.index = index;
                    scriptingDefineSymbolsList.onSelectCallback?.Invoke(scriptingDefineSymbolsList);
                }
            }

            string define = scriptingDefinesList[index];
            scriptingDefinesList[index] = EditorGUI.TextField(rect, scriptingDefinesList[index]);

            if (!scriptingDefinesList[index].Equals(define))
                SetScriptingDefinesListDirty();
        }

        void DrawTextFieldAdditionalCompilerArguments(Rect rect, int index)
        {
            // Handle list selection before the TextField grabs input
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                if (additionalCompilerArgumentsReorderableList.index != index)
                {
                    additionalCompilerArgumentsReorderableList.index = index;
                    additionalCompilerArgumentsReorderableList.onSelectCallback?.Invoke(additionalCompilerArgumentsReorderableList);
                }
            }

            string additionalCompilerArgument = additionalCompilerArgumentsList[index];
            additionalCompilerArgumentsList[index] = GUI.TextField(rect, additionalCompilerArgumentsList[index]);
            if (!additionalCompilerArgumentsList[index].Equals(additionalCompilerArgument))
                SetAdditionalCompilerArgumentListDirty();
        }

        void AddScriptingDefineCallback(ReorderableList list)
        {
            scriptingDefinesList.Add("");
            SetScriptingDefinesListDirty();
        }

        void RemoveScriptingDefineCallback(ReorderableList list)
        {
            scriptingDefinesList.RemoveAt(list.index);
            SetScriptingDefinesListDirty();
        }

        void DrawScriptingDefinesHeaderCallback(Rect rect)
        {
            using (new EditorGUI.PropertyScope(rect, GUIContent.none, m_ScriptingDefines))
            {
                GUI.Label(rect, SettingsContent.scriptingDefineSymbols, EditorStyles.label);
            }
        }

        void SetScriptingDefinesListDirty(ReorderableList list = null)
        {
            hasScriptingDefinesBeenModified = true;
        }

        void AddAdditionalCompilerArgumentCallback(ReorderableList list)
        {
            additionalCompilerArgumentsList.Add("");
            SetAdditionalCompilerArgumentListDirty();
        }

        void RemoveAdditionalCompilerArgumentCallback(ReorderableList list)
        {
            additionalCompilerArgumentsList.RemoveAt(list.index);
            SetAdditionalCompilerArgumentListDirty();
        }

        void SetAdditionalCompilerArgumentListDirty(ReorderableList list = null)
        {
            hasAdditionalCompilerArgumentsBeenModified = true;
        }

        private void OtherSectionOptimizationGUI(BuildPlatform platform, BuildTargetGroup targetGroup)
        {
            // Optimization
            GUILayout.Label(SettingsContent.optimizationTitle, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_BakeCollisionMeshes, SettingsContent.bakeCollisionMeshes);
            EditorGUILayout.PropertyField(m_KeepLoadedShadersAlive, SettingsContent.keepLoadedShadersAlive);

            if (isPreset)
                EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(m_PreloadedAssets, SettingsContent.preloadedAssets, true);

            if (isPreset)
                EditorGUI.indentLevel--;

            bool platformUsesAOT =
                targetGroup == BuildTargetGroup.iOS ||
                targetGroup == BuildTargetGroup.tvOS ||
                targetGroup == BuildTargetGroup.XboxOne ||
                targetGroup == BuildTargetGroup.PS4;

            if (platformUsesAOT)
                EditorGUILayout.PropertyField(m_AotOptions, SettingsContent.aotOptions);

            bool platformSupportsStripping = !BuildTargetDiscovery.PlatformGroupHasFlag(targetGroup, TargetAttributes.StrippingNotSupported);

            if (platformSupportsStripping)
            {
                ScriptingImplementation backend = GetCurrentBackendForTarget(targetGroup);
                if (BuildPipeline.IsFeatureSupported("ENABLE_ENGINE_CODE_STRIPPING", platform.defaultTarget) && backend == ScriptingImplementation.IL2CPP)
                    EditorGUILayout.PropertyField(m_StripEngineCode, SettingsContent.stripEngineCode);

                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    using (var propertyScope = new EditorGUI.PropertyScope(vertical.rect, GUIContent.none, m_ManagedStrippingLevel))
                    {
                        var availableStrippingLevels = GetAvailableManagedStrippingLevels(backend);
                        ManagedStrippingLevel currentManagedStrippingLevel = GetCurrentManagedStrippingLevelForTarget(targetGroup, backend);
                        ManagedStrippingLevel newManagedStrippingLevel;

                        newManagedStrippingLevel = BuildEnumPopup(SettingsContent.managedStrippingLevel, currentManagedStrippingLevel, availableStrippingLevels, GetNiceManagedStrippingLevelNames(availableStrippingLevels));
                        if (newManagedStrippingLevel != currentManagedStrippingLevel)
                            m_ManagedStrippingLevel.SetMapValue(BuildPipeline.GetBuildTargetGroupName(targetGroup), (int)newManagedStrippingLevel);
                    }
                }
            }

            if (targetGroup == BuildTargetGroup.iOS || targetGroup == BuildTargetGroup.tvOS)
            {
                EditorGUILayout.PropertyField(m_IPhoneScriptCallOptimization, SettingsContent.iPhoneScriptCallOptimization);
            }
            if (targetGroup == BuildTargetGroup.Android)
            {
                EditorGUILayout.PropertyField(m_AndroidProfiler, SettingsContent.enableInternalProfiler);
            }

            EditorGUILayout.Space();

            // Vertex compression flags dropdown
            VertexChannelCompressionFlags vertexFlags = (VertexChannelCompressionFlags)m_VertexChannelCompressionMask.intValue;
            vertexFlags = (VertexChannelCompressionFlags)EditorGUILayout.EnumFlagsField(SettingsContent.vertexChannelCompressionMask, vertexFlags);
            m_VertexChannelCompressionMask.intValue = (int)vertexFlags;

            EditorGUILayout.PropertyField(m_StripUnusedMeshComponents, SettingsContent.stripUnusedMeshComponents);
            EditorGUILayout.PropertyField(m_MipStripping, SettingsContent.mipStripping);

            EditorGUILayout.Space();
        }

        static ManagedStrippingLevel[] mono_levels = new ManagedStrippingLevel[] { ManagedStrippingLevel.Disabled, ManagedStrippingLevel.Low, ManagedStrippingLevel.Medium, ManagedStrippingLevel.High };
        static ManagedStrippingLevel[] il2cpp_levels = new ManagedStrippingLevel[] { ManagedStrippingLevel.Low, ManagedStrippingLevel.Medium, ManagedStrippingLevel.High };
        // stripping levels vary based on scripting backend
        private ManagedStrippingLevel[] GetAvailableManagedStrippingLevels(ScriptingImplementation backend)
        {
            if (backend == ScriptingImplementation.IL2CPP)
            {
                return il2cpp_levels;
            }
            else
            {
                return mono_levels;
            }
        }

        static Il2CppCompilerConfiguration[] m_Il2cppCompilerConfigurations;
        static GUIContent[] m_Il2cppCompilerConfigurationNames;

        private Il2CppCompilerConfiguration[] GetIl2CppCompilerConfigurations()
        {
            if (m_Il2cppCompilerConfigurations == null)
            {
                m_Il2cppCompilerConfigurations = new Il2CppCompilerConfiguration[]
                {
                    Il2CppCompilerConfiguration.Debug,
                    Il2CppCompilerConfiguration.Release,
                    Il2CppCompilerConfiguration.Master,
                };
            }

            return m_Il2cppCompilerConfigurations;
        }

        private GUIContent[] GetIl2CppCompilerConfigurationNames()
        {
            if (m_Il2cppCompilerConfigurationNames == null)
            {
                var configurations = GetIl2CppCompilerConfigurations();
                m_Il2cppCompilerConfigurationNames = new GUIContent[configurations.Length];

                for (int i = 0; i < configurations.Length; i++)
                    m_Il2cppCompilerConfigurationNames[i] = EditorGUIUtility.TextContent(configurations[i].ToString());
            }

            return m_Il2cppCompilerConfigurationNames;
        }

        public static bool IsLatestApiCompatibility(ApiCompatibilityLevel level)
        {
            return (level == ApiCompatibilityLevel.NET_4_6 || level == ApiCompatibilityLevel.NET_Standard_2_0);
        }

        private void OtherSectionLoggingGUI()
        {
            GUILayout.Label(SettingsContent.loggingTitle, EditorStyles.boldLabel);

            using (var vertical = new EditorGUILayout.VerticalScope())
            {
                using (var propertyScope = new EditorGUI.PropertyScope(vertical.rect, GUIContent.none, m_StackTraceTypes))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Log Type");
                        foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
                            GUILayout.Label(stackTraceLogType.ToString(), GUILayout.Width(70));
                    }

                    foreach (LogType logType in Enum.GetValues(typeof(LogType)))
                    {
                        var logProperty = m_StackTraceTypes.GetArrayElementAtIndex((int)logType);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(logType.ToString());
                            foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
                            {
                                StackTraceLogType inStackTraceLogType = (StackTraceLogType)logProperty.intValue;
                                EditorGUI.BeginChangeCheck();
                                bool val = EditorGUILayout.ToggleLeft(" ", inStackTraceLogType == stackTraceLogType, GUILayout.Width(70));
                                if (EditorGUI.EndChangeCheck() && val)
                                {
                                    logProperty.intValue = (int)stackTraceLogType;

                                    if (!isPreset)
                                        PlayerSettings.SetGlobalStackTraceLogType(logType, stackTraceLogType);
                                }
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void Stereo360CaptureGUI(BuildTargetGroup targetGroup)
        {
            EditorGUILayout.PropertyField(m_Enable360StereoCapture, SettingsContent.stereo360CaptureCheckbox);
        }

        private void OtherSectionLegacyGUI(BuildTargetGroup targetGroup)
        {
            GUILayout.Label(SettingsContent.legacyTitle, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(m_LegacyClampBlendShapeWeights, SettingsContent.legacyClampBlendShapeWeights);

            EditorGUILayout.Space();
        }

        private static Dictionary<ApiCompatibilityLevel, GUIContent> m_NiceApiCompatibilityLevelNames;
        private static Dictionary<ManagedStrippingLevel, GUIContent> m_NiceManagedStrippingLevelNames;

        private static GUIContent[] GetGUIContentsForValues<T>(Dictionary<T, GUIContent> contents, T[] values)
        {
            var names = new GUIContent[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (contents.ContainsKey(values[i]))
                    names[i] = contents[values[i]];
                else
                    throw new NotImplementedException(string.Format("Missing name for {0}", values[i]));
            }
            return names;
        }

        static GUIContent[] GetNiceScriptingBackendNames(ScriptingImplementation[] scriptingBackends)
        {
            return scriptingBackends.Select(s => GetNiceScriptingBackendName(s)).ToArray();
        }

        static GUIContent GetNiceScriptingBackendName(ScriptingImplementation scriptingBackend)
        {
            switch (scriptingBackend)
            {
                case ScriptingImplementation.Mono2x:
                    return SettingsContent.scriptingMono2x;
                case ScriptingImplementation.IL2CPP:
                    return SettingsContent.scriptingIL2CPP;
                default:
                    throw new ArgumentException($"Scripting backend value {scriptingBackend} is not supported.", nameof(scriptingBackend));
            }
        }

        private static GUIContent[] GetNiceApiCompatibilityLevelNames(ApiCompatibilityLevel[] apiCompatibilityLevels)
        {
            if (m_NiceApiCompatibilityLevelNames == null)
            {
                m_NiceApiCompatibilityLevelNames = new Dictionary<ApiCompatibilityLevel, GUIContent>
                {
                    { ApiCompatibilityLevel.NET_2_0, SettingsContent.apiCompatibilityLevel_NET_2_0 },
                    { ApiCompatibilityLevel.NET_2_0_Subset, SettingsContent.apiCompatibilityLevel_NET_2_0_Subset },
                    { ApiCompatibilityLevel.NET_4_6, SettingsContent.apiCompatibilityLevel_NET_4_6 },
                    { ApiCompatibilityLevel.NET_Standard_2_0, SettingsContent.apiCompatibilityLevel_NET_Standard_2_0 }
                };
            }

            return GetGUIContentsForValues(m_NiceApiCompatibilityLevelNames, apiCompatibilityLevels);
        }

        private static GUIContent[] GetNiceManagedStrippingLevelNames(ManagedStrippingLevel[] managedStrippingLevels)
        {
            if (m_NiceManagedStrippingLevelNames == null)
            {
                m_NiceManagedStrippingLevelNames = new Dictionary<ManagedStrippingLevel, GUIContent>
                {
                    { ManagedStrippingLevel.Disabled, SettingsContent.strippingDisabled },
                    { ManagedStrippingLevel.Low, SettingsContent.strippingLow },
                    { ManagedStrippingLevel.Medium, SettingsContent.strippingMedium },
                    { ManagedStrippingLevel.High, SettingsContent.strippingHigh },
                };
            }
            return GetGUIContentsForValues(m_NiceManagedStrippingLevelNames, managedStrippingLevels);
        }

        public void BrowseablePathProperty(string propertyLabel, SerializedProperty property, string browsePanelTitle, string extension, string dir)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(EditorGUIUtility.TextContent(propertyLabel));

            GUIContent browseBtnLabel = EditorGUIUtility.TrTextContent("...");
            Vector2 sizeOfLabel = GUI.skin.GetStyle("Button").CalcSize(browseBtnLabel);

            if (GUILayout.Button(browseBtnLabel, EditorStyles.miniButton, GUILayout.MaxWidth(sizeOfLabel.x)))
            {
                GUI.FocusControl("");

                string title = EditorGUIUtility.TempContent(browsePanelTitle).text;
                string currDirectory = string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory().Replace('\\', '/') + "/" : dir.Replace('\\', '/') + "/";
                string newStringValue = "";

                if (string.IsNullOrEmpty(extension))
                    newStringValue = EditorUtility.OpenFolderPanel(title, currDirectory, "");
                else
                    newStringValue = EditorUtility.OpenFilePanel(title, currDirectory, extension);

                if (newStringValue.StartsWith(currDirectory))
                    newStringValue = newStringValue.Substring(currDirectory.Length);

                if (!string.IsNullOrEmpty(newStringValue))
                {
                    property.stringValue = newStringValue;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUIContent gc = null;
            bool emptyString = string.IsNullOrEmpty(property.stringValue);
            using (new EditorGUI.DisabledScope(emptyString))
            {
                if (emptyString)
                {
                    gc = EditorGUIUtility.TrTextContent("Not selected.");
                }
                else
                {
                    gc = EditorGUIUtility.TempContent(property.stringValue);
                }

                EditorGUI.BeginChangeCheck();
                GUILayoutOption[] options = { GUILayout.Width(32), GUILayout.ExpandWidth(true) };
                string modifiedString = EditorGUILayout.TextArea(gc.text, options);
                if (EditorGUI.EndChangeCheck())
                {
                    if (string.IsNullOrEmpty(modifiedString))
                    {
                        property.stringValue = "";
                        serializedObject.ApplyModifiedProperties();
                        GUI.FocusControl("");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        internal static bool BuildPathBoxButton(SerializedProperty prop, string uiString, string directory)
        {
            return BuildPathBoxButton(prop, uiString, directory, null);
        }

        internal static bool BuildPathBoxButton(SerializedProperty prop, string uiString, string directory, Action onSelect)
        {
            float h = EditorGUI.kSingleLineHeight;
            float kLabelFloatMinW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
            float kLabelFloatMaxW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
            Rect r = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, h, h, EditorStyles.layerMaskField, null);

            float labelWidth = EditorGUIUtility.labelWidth;
            Rect buttonRect = new Rect(r.x + EditorGUI.indent, r.y, labelWidth - EditorGUI.indent, r.height);
            Rect fieldRect = new Rect(r.x + labelWidth, r.y, r.width - labelWidth, r.height);

            string display = (prop.stringValue.Length == 0) ? "Not selected." : prop.stringValue;
            EditorGUI.TextArea(fieldRect, display, EditorStyles.label);

            bool changed = false;
            if (GUI.Button(buttonRect, EditorGUIUtility.TextContent(uiString)))
            {
                string prevVal = prop.stringValue;
                string path = EditorUtility.OpenFolderPanel(EditorGUIUtility.TextContent(uiString).text, directory, "");

                string relPath = FileUtil.GetProjectRelativePath(path);
                prop.stringValue = (relPath != string.Empty) ? relPath : path;
                changed = (prop.stringValue != prevVal);

                if (onSelect != null)
                    onSelect();

                prop.serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        internal static bool BuildFileBoxButton(SerializedProperty prop, string uiString, string directory, string ext)
        {
            return BuildFileBoxButton(prop, uiString, directory, ext, null);
        }

        internal static bool BuildFileBoxButton(SerializedProperty prop, string uiString, string directory,
            string ext, Action onSelect)
        {
            bool changed = false;
            using (var vertical = new EditorGUILayout.VerticalScope())
            using (new EditorGUI.PropertyScope(vertical.rect, GUIContent.none, prop))
            {
                float h = EditorGUI.kSingleLineHeight;
                float kLabelFloatMinW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
                float kLabelFloatMaxW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
                Rect r = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, h, h, EditorStyles.layerMaskField, null);

                float labelWidth = EditorGUIUtility.labelWidth;
                Rect buttonRect = new Rect(r.x + EditorGUI.indent, r.y, labelWidth - EditorGUI.indent, r.height);
                Rect fieldRect = new Rect(r.x + labelWidth, r.y, r.width - labelWidth, r.height);

                string display = (prop.stringValue.Length == 0) ? "Not selected." : prop.stringValue;
                EditorGUI.TextArea(fieldRect, display, EditorStyles.label);

                if (GUI.Button(buttonRect, EditorGUIUtility.TextContent(uiString)))
                {
                    string prevVal = prop.stringValue;
                    string path = EditorUtility.OpenFilePanel(EditorGUIUtility.TextContent(uiString).text, directory, ext);

                    string relPath = FileUtil.GetProjectRelativePath(path);
                    prop.stringValue = (relPath != string.Empty) ? relPath : path;
                    changed = (prop.stringValue != prevVal);

                    if (onSelect != null)
                        onSelect();

                    prop.serializedObject.ApplyModifiedProperties();
                }
            }

            return changed;
        }

        public void PublishSectionGUI(BuildTargetGroup targetGroup, ISettingEditorExtension settingsExtension, int sectionIndex = 5)
        {
            if (targetGroup != BuildTargetGroup.WSA &&
                !(settingsExtension != null && settingsExtension.HasPublishSection()))
                return;

            if (BeginSettingsBox(sectionIndex, SettingsContent.publishingSettingsTitle))
            {
                float h = EditorGUI.kSingleLineHeight;
                float kLabelFloatMinW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;
                float kLabelFloatMaxW = EditorGUI.kLabelW + EditorGUIUtility.fieldWidth + EditorGUI.kSpacing;

                if (settingsExtension != null)
                {
                    settingsExtension.PublishSectionGUI(h, kLabelFloatMinW, kLabelFloatMaxW);
                }
            }
            EndSettingsBox();
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }

        [SettingsProvider]
        internal static SettingsProvider CreateProjectSettingsProvider()
        {
            var provider = AssetSettingsProvider.CreateProviderFromAssetPath(
                "Project/Player", "ProjectSettings/ProjectSettings.asset",
                SettingsProvider.GetSearchKeywordsFromGUIContentProperties<SettingsContent>());
            provider.activateHandler = (searchContext, rootElement) =>
            {
                var playerSettingsProvider = provider.settingsEditor as PlayerSettingsEditor;
                if (playerSettingsProvider != null)
                {
                    playerSettingsProvider.SetValueChangeListeners(provider.Repaint);
                    playerSettingsProvider.splashScreenEditor.SetValueChangeListeners(provider.Repaint);
                }
            };
            return provider;
        }

        void InitReorderableScriptingDefineSymbolsList(BuildTargetGroup targetGroup)
        {
            scriptingDefinesList = new List<string>(PlayerSettings.ConvertScriptingDefineStringToArray(serializedScriptingDefines));

            scriptingDefineSymbolsList = new ReorderableList(scriptingDefinesList, typeof(string), true, true, true, true);
            scriptingDefineSymbolsList.drawElementCallback = (rect, index, isActive, isFocused) => DrawTextField(rect, index);
            scriptingDefineSymbolsList.drawHeaderCallback = (rect) => DrawScriptingDefinesHeaderCallback(rect);
            scriptingDefineSymbolsList.onAddCallback = AddScriptingDefineCallback;
            scriptingDefineSymbolsList.onRemoveCallback = RemoveScriptingDefineCallback;
            scriptingDefineSymbolsList.onChangedCallback = SetScriptingDefinesListDirty;
        }

        void UpdateScriptingDefineSymbolsLists()
        {
            scriptingDefinesList = new List<string>(PlayerSettings.ConvertScriptingDefineStringToArray(serializedScriptingDefines));
            scriptingDefineSymbolsList.list = scriptingDefinesList;
            scriptingDefineSymbolsList.DoLayoutList();
            hasScriptingDefinesBeenModified = false;
        }

        void InitReorderableAdditionalCompilerArgumentsList(BuildTargetGroup targetGroup)
        {
            additionalCompilerArgumentsList = new List<string>(serializedAdditionalCompilerArguments);

            additionalCompilerArgumentsReorderableList = new ReorderableList(additionalCompilerArgumentsList, typeof(string), true, true, true, true);
            additionalCompilerArgumentsReorderableList.drawElementCallback = (rect, index, isActive, isFocused) => DrawTextFieldAdditionalCompilerArguments(rect, index);
            additionalCompilerArgumentsReorderableList.drawHeaderCallback = (rect) => GUI.Label(rect, SettingsContent.additionalCompilerArguments, EditorStyles.label);
            additionalCompilerArgumentsReorderableList.onAddCallback = AddAdditionalCompilerArgumentCallback;
            additionalCompilerArgumentsReorderableList.onRemoveCallback = RemoveAdditionalCompilerArgumentCallback;
            additionalCompilerArgumentsReorderableList.onChangedCallback = SetAdditionalCompilerArgumentListDirty;
        }

        void UpdateAdditionalCompilerArgumentsLists()
        {
            additionalCompilerArgumentsList = new List<string>(serializedAdditionalCompilerArguments);
            additionalCompilerArgumentsReorderableList.list = additionalCompilerArgumentsList;
            additionalCompilerArgumentsReorderableList.DoLayoutList();
            hasAdditionalCompilerArgumentsBeenModified = false;
        }
    }
}
