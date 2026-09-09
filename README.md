# RainyFetch

![](https://img.shields.io/badge/.Net-8.0-lightgrey?style=for-the-badge&logo=windows)&ensp;
[![](https://img.shields.io/github/v/release/lzcapp/RainyFetch?style=for-the-badge)](https://github.com/lzcapp/RainyFetch/releases/latest)
[![build](https://github.com/lzcapp/RainyFetch/actions/workflows/build.yml/badge.svg)](https://github.com/lzcapp/RainyFetch/actions/workflows/build.yml)

A Windows system-information fetcher (neofetch-style) for the console.
Collects and renders CPU / GPU / memory / disks / NICs / motherboard / BIOS / OS
details via WMI (`System.Management`), with an ASCII logo in red-and-white.

![Screenshot](https://github.com/lzcapp/RainyFetch/assets/12462465/22c3d413-fcbe-408e-9bc8-5affdd68973c)

## Requirements

- Windows 10 / 11 (x64 or x86). WMI-only, no admin rights needed.
- For the **framework-dependent** builds: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime?cid=getdotnetcore).

Releases ship four executables (see the Releases page asset naming):

| Asset                    | Arch | Runtime needed        |
|--------------------------|------|-----------------------|
| `RainyFetch_x64.exe`     | x64  | yes — .NET 8 Desktop  |
| `RainyFetch_x64_runtime.exe` | x64 | **no** (self-contained) |
| `RainyFetch_x86.exe`     | x86  | yes — .NET 8 Desktop  |
| `RainyFetch_x86_runtime.exe` | x86 | **no** (self-contained) |

The `_runtime` ones are larger self-contained single files that run anywhere;
the plain ones are small and need the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime?cid=getdotnetcore).

## Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```console
dotnet restore RainyFetch.csproj
dotnet build RainyFetch.csproj -c Release
```

The output lands in `bin\Release\net8.0-windows\RainyFetch.exe`.

## Publish

Four flavors are published (also by CI), matching the Releases asset naming
(`RainyFetch_<arch>.exe` = framework-dependent, `RainyFetch_<arch>_runtime.exe` =
self-contained single file; only the apphost exe is renamed, the `RainyFetch.dll`
and `.deps.json` keep their names):

```console
REM win-x64, framework-dependent (small; needs the .NET 8 runtime)
dotnet publish RainyFetch.csproj -c Release -r win-x64 --self-contained false -o bin\publish\x64
REM then rename RainyFetch.exe -> RainyFetch_x64.exe

REM win-x64, self-contained single file (~65 MB; runs anywhere)
dotnet publish RainyFetch.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o bin\publish\x64-runtime
REM then rename RainyFetch.exe -> RainyFetch_x64_runtime.exe

REM win-x86, framework-dependent
dotnet publish RainyFetch.csproj -c Release -r win-x86 --self-contained false -o bin\publish\x86

REM win-x86, self-contained single file (~60 MB; runs anywhere)
dotnet publish RainyFetch.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o bin\publish\x86-runtime
```

> Trimming is intentionally **not** used: `System.Management` (WMI) relies on
> reflection/COM interop that is unsafe under `PublishTrimmed`.
>
> `Properties\PublishProfiles\*.pubxml` are local-only helpers for Visual Studio's
> Publish dialog (git-ignored by the standard VS `.gitignore`); the commands above are
> the canonical way to publish.

## CI

`.github/workflows/build.yml` builds, smoke-runs and publishes **all four flavors** on every
push/PR to `main` (GitHub Actions, `windows-latest`). Artifacts are uploaded under the same
names as the Releases assets: `RainyFetch_x64`, `RainyFetch_x64_runtime`,
`RainyFetch_x86`, `RainyFetch_x86_runtime`.

Pushing a tag (e.g. `2.4.0`) additionally creates the GitHub Release itself:
the four executables are attached and the release notes are generated from the
commits since the previous tag.

No command line needed either: **Actions → build → Run workflow → enter the version**
creates that tag, builds and publishes the release in one go.

## Notes

- Output is written using the console code page (e.g. GBK on a Chinese-locale system).
  When you **redirect** output to a file and it looks garbled in a UTF-8 editor,
  reopen the file with the system ANSI/GBK encoding — this matches the classic .NET
  Framework behavior the tool is a port of.
- Colors are skipped automatically when output is redirected.
