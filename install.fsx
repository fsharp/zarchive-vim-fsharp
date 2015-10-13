// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System
open System.IO
open System.Net
open System.Text.RegularExpressions

let homeVimPath =
    if Environment.OSVersion.Platform = PlatformID.Unix || Environment.OSVersion.Platform = PlatformID.MacOSX then
        Environment.GetEnvironmentVariable("HOME") @@ ".vim"
    else Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") @@ "vimfiles"

let vimInstallDir = homeVimPath @@ "bundle/fsharpbinding-vim"

let vimBinDir = __SOURCE_DIRECTORY__ @@ "ftplugin/bin"
let ftpluginDir = __SOURCE_DIRECTORY__ @@ "ftplugin"
let autoloadDir = __SOURCE_DIRECTORY__ @@ "autoload"
let syntaxDir = __SOURCE_DIRECTORY__ @@ "syntax"
let ftdetectDir = __SOURCE_DIRECTORY__ @@ "ftdetect"
let syntaxCheckersDir = __SOURCE_DIRECTORY__ @@ "syntax_checkers"

let fsharpVim = ftpluginDir @@ "fsharp.vim"

let acArchive = "fsautocomplete.zip"
let acVersion = "0.23.0"

let buildExecPath =
  lazy
    if isMono then "xbuild" else
      let keyString = "SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions"

      let mostRecentVersionKey =
        match RegistryHelper.getRegistryKey HKEYLocalMachine keyString false with
        | null -> failwith "MSBuild could not be found in the System Registry (no ToolsVersion installed)."
        | key ->
            match key.GetSubKeyNames () with
            | [||] -> failwith "MSBuild could not be found in the System Registry (no ToolsVersion installed)."
            | versions -> versions
                          |> Array.maxBy System.Version
                          |> sprintf "%s\\%s" keyString

      if valueExistsForKey HKEYLocalMachine mostRecentVersionKey "MSBuildToolsPath"
      then
        let executable =
          RegistryHelper.getRegistryValue HKEYLocalMachine mostRecentVersionKey "MSBuildToolsPath"
            @@ "MSbuild.exe" |> normalizePath

        if fileExists executable then executable
        else failwithf "File not found: %s" executable
      else failwithf "Registry key 'MSBuildToolsPath' missing in: HKEY_LOCAL_MACHINE\\%s." mostRecentVersionKey

let buildExecutable =
  // In Windows, 'shellescape' is needed to escape the path for vim
  let pathEscape = if isMono then sprintf "'%s'" else sprintf "shellescape('%s')"

  // The build executable can be set as an environment variable.
  // For example: ./install.cmd -ev build "path/to/msbuild"
  pathEscape (if hasBuildParam "build" then getBuildParam "build" else buildExecPath.Value)

let buildExecutableTag = "{BUILD_EXECUTABLE}"

Target "FSharp.AutoComplete" (fun _ ->
  CreateDir vimBinDir
  use client = new WebClient()
  tracefn "Downloading version %s of FSharp.AutoComplete" acVersion
  client.DownloadFile(sprintf "https://github.com/fsharp/FSharp.AutoComplete/releases/download/%s/%s" acVersion acArchive, vimBinDir @@ acArchive)
  tracefn "Download complete"
  tracefn "Unzipping"
  Unzip vimBinDir (vimBinDir @@ acArchive))

Target "AddMSBuildPath" (fun _ ->
    CopyFile fsharpVim (fsharpVim + ".template")
    processTemplates [buildExecutableTag, buildExecutable] [fsharpVim])

Target "CleanMSBuildPath" (fun _ ->
    CopyFile fsharpVim (fsharpVim + ".template")
    processTemplates [buildExecutableTag, "'xbuild'"] [fsharpVim])

Target "Install" (fun _ ->
    DeleteDir vimInstallDir
    CreateDir vimInstallDir
    CopyDir (vimInstallDir @@ "ftplugin") ftpluginDir (function EndsWith ".template" -> false | _ -> true)
    CopyDir (vimInstallDir @@ "autoload") autoloadDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "syntax") syntaxDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "syntax_checkers") syntaxCheckersDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "ftdetect") ftdetectDir (fun _ -> true))

Target "Clean" (fun _ ->
    CleanDirs [ vimBinDir; vimInstallDir ])

Target "All" id

"CleanMSBuildPath"
    ==> "Clean"

"AddMSBuildPath"
    ==> "FSharp.AutoComplete"
    ==> "Install"
    ==> "All"

RunTargetOrDefault "All"
