#r "packages/FAKE.1.74.131.0/tools/FakeLib.dll"
#r "packages/IntelliFactory.Build.0.0.2/lib/net40/IntelliFactory.Build.dll"

open System
open System.IO
open Fake
module B = IntelliFactory.Build.CommonBuildSetup

let Metadata =
    let m = B.Metadata.Create()
    m.Author <- Some "IntelliFactory"
    m.AssemblyVersion <- Some (Version "0.0.0.0")
    m.FileVersion <- Some (Version "0.0.4.0")
    m.Description <- Some "Utilities for faster delegate invocation"
    m.Product <- Some "IntelliFactory.FastInvoke"
    m.Website <- Some "http://bitbucket.org/IntelliFactory/fastinvoke"
    m

let Frameworks = [B.Net20; B.Net40]

let Solution =
    B.Solution.Standard __SOURCE_DIRECTORY__ Metadata [
        B.Project.FSharp "IntelliFactory.FastInvoke" Frameworks
    ]

Target "Build" Solution.Build
Target "Clean" Solution.Clean

match Environment.GetCommandLineArgs() with
| xs when xs.[xs.Length - 1] = "Clean" -> RunTargetOrDefault "Clean"
| _ -> RunTargetOrDefault "Build"

