#r @"packages/FAKE.3.5.4/tools/FakeLib.dll"
#load "build-helpers.fsx"
open Fake
open System
open System.IO
open System.Linq
open BuildHelpers
open Fake.XamarinHelper

Target "restore-nuget" (fun () ->
    RestorePackagesOutput "../SimpleAuth/samples/SimpleAuth.Samples.sln" "../SimpleAuth/samples/packages/"
    RestorePackagesOutput "../SimpleAuth/src/SimpleAuth.sln" "../SimpleAuth/src/packages/"
    RestorePackages "MusicPlayer.sln"
)

Target "ios-build" (fun () ->
    RestorePackagesOutput "../SimpleAuth/samples/SimpleAuth.Samples.sln" "../SimpleAuth/samples/packages/"
    RestorePackagesOutput "../SimpleAuth/src/SimpleAuth.sln" "../SimpleAuth/src/packages/"
    RestorePackages "MusicPlayer.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "MusicPlayer.sln"
            Configuration = "Debug|iPhoneSimulator"
            Target = "Build"
        })
)

Target "ios-adhoc" (fun () ->
    RestorePackages "../SimpleAuth/samples/SimpleAuth.Samples.sln"
    RestorePackages "../SimpleAuth/src/SimpleAuth.sln"
    RestorePackages "MusicPlayer.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "MusicPlayer.sln"
            Configuration = "Ad-Hoc|iPhone"
            Target = "Build"
        })

    let appPath = Directory.EnumerateFiles(Path.Combine("src", "MusicPlayer", "bin", "iPhone", "Ad-Hoc"), "*.ipa").First()

    TeamCityHelper.PublishArtifact appPath
)

Target "ios-appstore" (fun () ->
    RestorePackages "../SimpleAuth/samples/SimpleAuth.Samples.sln"
    RestorePackages "../SimpleAuth/src/SimpleAuth.sln"
    RestorePackages "MusicPlayer.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "MusicPlayer.sln"
            Configuration = "AppStore|iPhone"
            Target = "Build"
        })

    let outputFolder = Path.Combine("src", "MusicPlayer", "bin", "iPhone", "AppStore")
    let appPath = Directory.EnumerateDirectories(outputFolder, "*.app").First()
    let zipFilePath = Path.Combine(outputFolder, "MusicPlayer.zip")
    let zipArgs = String.Format("-r -y '{0}' '{1}'", zipFilePath, appPath)

    Exec "zip" zipArgs

    TeamCityHelper.PublishArtifact zipFilePath
)

//Target "ios-uitests" (fun () ->
//    let appPath = Directory.EnumerateDirectories(Path.Combine("src", "TipCalc.iOS", "bin", "iPhoneSimulator", "Debug"), "*.app").First()
//
//    RunUITests appPath
//)

Target "ios-testcloud" (fun () ->
    RestorePackages "../SimpleAuth/samples/SimpleAuth.Samples.sln"
    RestorePackages "MusicPlayer.sln"

    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = "MusicPlayer.sln"
            Configuration = "TestCloud|iPhone"
            Target = "Build"
        })

    let appPath = Directory.EnumerateFiles(Path.Combine( "MusicPlayer.iOS", "bin", "iPhone", "TestCloud"), "*.ipa").First()

    getBuildParam "devices" |> RunTestCloudTests appPath
)

//Target "android-build" (fun () ->
//    RestorePackages "TipCalc.Android.sln"
//
//    MSBuild "src/TipCalc.Android/bin/Release" "Build" [ ("Configuration", "Release") ] [ "TipCalc.Android.sln" ] |> ignore
//)
//
//Target "android-package" (fun () ->
//    AndroidPackage (fun defaults ->
//        {defaults with
//            ProjectPath = "src/TipCalc.Android/TipCalc.Android.csproj"
//            Configuration = "Release"
//            OutputPath = "src/TipCalc.Android/bin/Release"
//        }) 
//    |> AndroidSignAndAlign (fun defaults ->
//        {defaults with
//            KeystorePath = "tipcalc.keystore"
//            KeystorePassword = "tipcalc" // TODO: don't store this in the build script for a real app!
//            KeystoreAlias = "tipcalc"
//        })
//    |> fun file -> TeamCityHelper.PublishArtifact file.FullName
//)
//
//Target "android-uitests" (fun () ->
//    AndroidPackage (fun defaults ->
//        {defaults with
//            ProjectPath = "src/TipCalc.Android/TipCalc.Android.csproj"
//            Configuration = "Release"
//            OutputPath = "src/TipCalc.Android/bin/Release"
//        }) |> ignore
//
//    let appPath = Directory.EnumerateFiles(Path.Combine("src", "TipCalc.Android", "bin", "Release"), "*.apk", SearchOption.AllDirectories).First()
//
//    RunUITests appPath
//)
//
//Target "android-testcloud" (fun () ->
//    AndroidPackage (fun defaults ->
//        {defaults with
//            ProjectPath = "src/TipCalc.Android/TipCalc.Android.csproj"
//            Configuration = "Release"
//            OutputPath = "src/TipCalc.Android/bin/Release"
//        }) |> ignore
//
//    let appPath = Directory.EnumerateFiles(Path.Combine("src", "TipCalc.Android", "bin", "Release"), "*.apk", SearchOption.AllDirectories).First()
//
//    getBuildParam "devices" |> RunTestCloudTests appPath
//)

//"core-build"
//  ==> "core-tests"
//
//"ios-build"
//  ==> "ios-uitests"

//"android-build"
//  ==> "android-uitests"
//
//"android-build"
//  ==> "android-testcloud"
//
//"android-build"
//  ==> "android-package"

RunTarget() 