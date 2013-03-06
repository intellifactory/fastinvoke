#r "packages/FAKE.1.74.127.0/tools/FakeLib.dll"

open System
open System.IO
open Fake

module Config =
    let FileVersion = Version("0.0.3.0")
    let AssemblyVersion = Version("0.0.0.0")
    let Product = "IntelliFactory.FastInvoke"
    let Repo = "http://bitbucket.org/IntelliFactory/fastinvoke"

let Root = __SOURCE_DIRECTORY__
let ( +/ ) a b = System.IO.Path.Combine(a, b)

/// Infers the current Mercurial revision from the `.hg` folder.
let InferTag () =
    let bytes = File.ReadAllBytes(Root +/ ".hg" +/ "dirstate")
    Array.sub bytes 0 20
    |> Array.map (fun b -> String.Format("{0:x2}", b))
    |> String.concat ""

[<AutoOpen>]
module Tagging =
    type private A = AssemblyInfoFile.Attribute

    let PrepareAssemblyInfo =
        Target "PrepareAssemblyInfo" <| fun () ->
            let tag = InferTag ()
            let buildDir = Root +/ ".build"
            ensureDirectory buildDir
            let fsInfo = buildDir +/ "AutoAssemblyInfo.fs"
            let csInfo = buildDir +/ "AutoAssemblyInfo.cs"
            let desc =
                String.Format("See \
                    the source code at <{2}>. \
                    Mercurial tag: {0}. Build date: {1}", tag, DateTimeOffset.UtcNow, Config.Repo)
            let attrs =
                [
                    A.Company "IntelliFactory"
                    A.Copyright (String.Format("(c) {0} IntelliFactory", DateTime.Now.Year))
                    A.FileVersion (string Config.FileVersion)
                    A.Description desc
                    A.Product (String.Format("{0} (tag: {1})", Config.Product, tag))
                    A.Version (string Config.AssemblyVersion)
                ]
            AssemblyInfoFile.CreateFSharpAssemblyInfo fsInfo attrs
            AssemblyInfoFile.CreateCSharpAssemblyInfo csInfo attrs

let Projects =
    !+ (Root +/ "*" +/ "*.csproj")
    ++ (Root +/ "*" +/ "*.fsproj")
    |> Scan

type Framework =
    | V20
    | V40

let Frameworks = [V20; V40]

let Properties (f: Framework) (project: string) =
    [
        "Configuration", "Release"
        "Framework",
            match f with
            | V20 -> "V2.0"
            | V40 -> "v4.0"
    ]

Target "Build" <| fun () ->
    tracefn "Building"
    for f in Frameworks do
        MSBuildWithProjectProperties "" "Build" (Properties f) Projects
        |> ignore

Target "Clean" <| fun () ->
    tracefn "Cleaning"
    DeleteDir (Root +/ ".build")
    for f in Frameworks do
        MSBuildWithProjectProperties "" "Clean" (Properties f) Projects
        |> ignore

"Clean" ==> "PrepareAssemblyInfo" ==> "Build"

RunTargetOrDefault "Build"
