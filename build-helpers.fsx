module BuildHelpers

open Fake
open Fake.XamarinHelper
open System
open System.IO
open System.Linq

let Exec command args =
    let result = Shell.Exec(command, args)

    if result <> 0 then failwithf "%s exited with error %d" command result

let RestorePackagesOutput solutionFile outputFolder =
    Exec "tools/NuGet/NuGet.exe" ("restore " + solutionFile + " -PackagesDirectory " + outputFolder)
    solutionFile |> RestoreComponents (fun defaults -> {defaults with ToolPath = "tools/xpkg/xamarin-component.exe" })

let RestorePackages solutionFile =
    Exec "tools/NuGet/NuGet.exe" ("restore " + solutionFile)
    solutionFile |> RestoreComponents (fun defaults -> {defaults with ToolPath = "tools/xpkg/xamarin-component.exe" })

let RunNUnitTests dllPath xmlPath =
    Exec "/Library/Frameworks/Mono.framework/Versions/Current/bin/nunit-console4" (dllPath + " -xml=" + xmlPath)
    TeamCityHelper.sendTeamCityNUnitImport xmlPath

let RunUITests appPath =
    let testAppFolder = Path.Combine("Test", "TestCloud.iOS", "testapps")
    
    if Directory.Exists(testAppFolder) then Directory.Delete(testAppFolder, true)
    Directory.CreateDirectory(testAppFolder) |> ignore

    let testAppPath = Path.Combine(testAppFolder, DirectoryInfo(appPath).Name)

    Directory.Move(appPath, testAppPath)


    MSBuild "Test/TestCloud.iOS/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] [ "Test/TestCloud.iOS/TestCloud.iOS.csproj" ] |> ignore

    RunNUnitTests "Test/TestCloud.iOS/bin/Debug/TestCloud.iOS.dll" "tests/TestCloud.iOS/bin/Debug/testresults.xml"

let RunTestCloudTests appFile deviceList =
    MSBuild "Test/TestCloud.iOS/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] [ "Test/TestCloud.iOS/TestCloud.iOS.csproj" ] |> ignore

    let testCloudToken = "560a260a4c7544bf5076b037cbd04c18"//Environment.GetEnvironmentVariable("TestCloudApiToken")
    let args = String.Format(@"submit ""{0}"" {1} --devices {2} --series ""master""  --app-name ""gMusic beta"" --user james.clancey@xamarin.com --fixture-chunk --locale ""en_US"" --assembly-dir ""Test/TestCloud.iOS/bin/Debug"" --nunit-xml Test/TestCloud.iOS/bin/Debug/testresults.xml", appFile, testCloudToken, deviceList)

    Exec "packages/Xamarin.UITest.1.3.3/tools/test-cloud.exe" args

    TeamCityHelper.sendTeamCityNUnitImport "Test/TestCloud.iOS/bin/Debug/testresults.xml"