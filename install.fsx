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

let ftpluginDir = __SOURCE_DIRECTORY__ @@ "ftplugin"
let autoloadDir = __SOURCE_DIRECTORY__ @@ "autoload"
let syntaxDir = __SOURCE_DIRECTORY__ @@ "syntax"
let ftdetectDir = __SOURCE_DIRECTORY__ @@ "ftdetect"
let syntaxCheckersDir = __SOURCE_DIRECTORY__ @@ "syntax_checkers"

let vimACDir = vimInstallDir @@ "ftplugin" @@ "bin"
// FSAutoComplete files downloaded with Paket
let ACFiles = !! "paket-files/github.com/**/*.*"
              -- "*.zip"
              -- "paket.version"

Target "Install" (fun _ ->
    DeleteDir vimInstallDir
    CreateDir vimInstallDir
    CopyDir (vimInstallDir @@ "ftplugin") ftpluginDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "autoload") autoloadDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "syntax") syntaxDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "syntax_checkers") syntaxCheckersDir (fun _ -> true)
    CopyDir (vimInstallDir @@ "ftdetect") ftdetectDir (fun _ -> true)
    CreateDir vimACDir
    Copy vimACDir ACFiles)

Target "Clean" (fun _ ->
    CleanDirs [ vimInstallDir ])

Target "All" id

"Install"
    ==> "All"

RunTargetOrDefault "All"
