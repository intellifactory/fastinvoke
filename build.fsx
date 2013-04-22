#if BOOT
open Fake
module FB = Fake.Boot
FB.Prepare {
    FB.Config.Default __SOURCE_DIRECTORY__ with
        NuGetDependencies =
            let ( ! ) x = FB.NuGetDependency.Create x
            [
                !"IntelliFactory.Build"
            ]
}
#else
#load ".build/boot.fsx"
open Fake

open System
open System.IO
open System.Net
open System.Text

module B = IntelliFactory.Build.CommonBuildSetup
module F = IntelliFactory.Build.FileSystem
module NG = IntelliFactory.Build.NuGetUtils
module VST = IntelliFactory.Build.VSTemplates

let ( +/ ) a b = Path.Combine(a, b)
let RootDir = __SOURCE_DIRECTORY__
let T x f = Target x f; x

module Config =

    let PackageId = "IntelliFactory.FastInvoke"
    let AssemblyVersion = Version "0.0"
    let NuGetVersion = NG.ComputeVersion PackageId (global.NuGet.SemanticVersion AssemblyVersion)
    let FileVersion = NuGetVersion.Version

    let Company = "IntelliFactory"
    let Description =
        "Provides a faster alternative to calling `.Invoke` on \
        `System.Reflection.MethodInfo` by separating the binding and invocation stages."
    let LicenseUrl = "http://websharper.com/licensing"
    let Tags = "F# Delegate Invocation".Split(' ') |> Array.toList
    let Website = "http://bitbucket.org/IntelliFactory/fastinvoke"

let Metadata =
    let m = B.Metadata.Create()
    m.AssemblyVersion <- Some Config.AssemblyVersion
    m.Author <- Some Config.Company
    m.Description <- Some Config.Description
    m.FileVersion <- Some Config.FileVersion
    m.Product <- Some Config.PackageId
    m.Website <- Some Config.Website
    m

[<AutoOpen>]
module Extensions =

    type B.BuildConfiguration with
        static member Release(v: B.FrameworkVersion) : B.BuildConfiguration =
            {
                ConfigurationName = "Release"
                Debug = false
                FrameworkVersion = v
                NuGetDependencies = new global.NuGet.PackageDependencySet(v.ToFrameworkName(), [])
            }

    type B.Solution with

        static member Standard(rootDir: string)(m: B.Metadata)(ps: list<string -> B.Project>) : B.Solution =
            {
                Metadata = m
                Projects = [for p in ps -> p rootDir]
                RootDirectory = rootDir
            }

        member this.BuildSync(?opts: B.MSBuildOptions) =
            this.MSBuild(?options=opts)
            |> Async.RunSynchronously

        member this.CleanSync(?opts: B.MSBuildOptions) =
            let opts : B.MSBuildOptions =
                match opts with
                | Some opts ->
                    { opts with Targets = ["Clean"] }
                | None ->
                    {
                        BuildConfiguration = None
                        Properties = Map.empty
                        Targets = ["Clean"]
                    }
            this.MSBuild opts
            |> Async.RunSynchronously

    type B.Project with

        static member FSharp(name: string)(configs: list<B.BuildConfiguration>)(rootDir: string) : B.Project =
            {
                Name = name
                MSBuildProjectFilePath = Some (rootDir +/ name +/ (name + ".fsproj"))
                BuildConfigurations = configs
            }

        static member CSharp(name: string)(configs: list<B.BuildConfiguration>)(rootDir: string) : B.Project =
            {
                Name = name
                MSBuildProjectFilePath = Some (rootDir +/ name +/ (name + ".csproj"))
                BuildConfigurations = configs
            }

let C20 = B.BuildConfiguration.Release B.Net20
let C40 = B.BuildConfiguration.Release B.Net40

let Configs = [C20; C40]

let Solution : B.Solution =
    B.Solution.Standard RootDir Metadata [
        B.Project.FSharp "IntelliFactory.FastInvoke" Configs
    ]

/// TODO: helpers for buliding packages from a solution spec.
let BuildNuGetPackage = T "BuildNuGetPackage" <| fun () ->
    let content =
        use out = new MemoryStream()
        let builder = new NuGet.PackageBuilder()
        builder.Id <- Config.PackageId
        builder.Version <- Config.NuGetVersion
        builder.Authors.Add(Config.Company) |> ignore
        builder.Owners.Add(Config.Company) |> ignore
        builder.LicenseUrl <- Uri(Config.LicenseUrl)
        builder.ProjectUrl <- Uri(Config.Website)
        builder.Copyright <- String.Format("Copyright (c) {0} {1}", DateTime.Now.Year, Config.Company)
        builder.Description <- Config.Description
        Config.Tags
        |> Seq.iter (builder.Tags.Add >> ignore)
        for c in Configs do
            for ext in [".xml"; ".dll"] do
                let n = Config.PackageId
                builder.Files.Add
                    (
                        let f = new NuGet.PhysicalPackageFile()
                        f.SourcePath <- RootDir +/ n +/ "bin" +/ ("Release-" + c.FrameworkVersion.GetMSBuildLiteral()) +/ (n + ext)
                        f.TargetPath <- "lib" +/ c.FrameworkVersion.GetNuGetLiteral() +/ (n + ext)
                        f
                    )
        builder.Save(out)
        F.Binary.FromBytes (out.ToArray())
        |> F.BinaryContent
    let out = RootDir +/ ".build" +/ String.Format("{0}.{1}.nupkg", Config.PackageId, Config.NuGetVersion)
    content.WriteFile(out)
    tracefn "Written %s" out

let BuildSolution = T "BuildSolution" Solution.BuildSync
let CleanSolution = T "CleanSolution" Solution.CleanSync

let Build = T "Build" ignore
let Clean = T "Clean" ignore

BuildSolution
==> BuildNuGetPackage
==> Build

CleanSolution
==> Clean

let Prepare = T "Prepare" <| fun () ->
    B.Prepare (tracefn "%s") RootDir

RunTargetOrDefault Build

#endif
