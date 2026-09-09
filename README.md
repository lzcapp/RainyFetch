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

Releases ship two flavors: small executables **without "_runtime"** (framework-dependent,
needs the runtime above) and larger ones **with "_runtime"** (self-contained single file,
runs anywhere).

## Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

```console
dotnet restore RainyFetch.csproj
dotnet build RainyFetch.csproj -c Release
```

The output lands in `bin\Release\net8.0-windows\RainyFetch.exe`.

## Publish

Two flavors are published (also by CI):

```console
REM win-x64, framework-dependent (small; needs the .NET 8 runtime)
dotnet publish RainyFetch.csproj -c Release -r win-x64 --self-contained false -o bin\publish\win-x64

REM win-x86, self-contained single file (~60 MB; runs anywhere, no runtime needed)
dotnet publish RainyFetch.csproj -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -o bin\publish\win-x86
```

> Trimming is intentionally **not** used: `System.Management` (WMI) relies on
> reflection/COM interop that is unsafe under `PublishTrimmed`.
>
> `Properties\PublishProfiles\*.pubxml` are local-only helpers for Visual Studio's
> Publish dialog (git-ignored by the standard VS `.gitignore`); the commands above are
> the canonical way to publish.

## CI

`.github/workflows/build.yml` builds, smoke-runs and publishes both flavors on every push/PR
to `main` (GitHub Actions, `windows-latest`). Artifacts are uploaded as
`RainyFetch-win-x64-fdd` and `RainyFetch-win-x86-singlefile`.

## Notes

- Output is written using the console code page (e.g. GBK on a Chinese-locale system).
  When you **redirect** output to a file and it looks garbled in a UTF-8 editor,
  reopen the file with the system ANSI/GBK encoding — this matches the classic .NET
  Framework behavior the tool is a port of.
- Colors are skipped automatically when output is redirected.
