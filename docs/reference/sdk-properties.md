# SDK Properties

Mod projects use the Lustral SDK:

```xml
<Project Sdk="Lustral.Sdk/0.1.0">
  <PropertyGroup>
    <LustralGame>Lethal Company</LustralGame>
    <ModId>you.betterhud</ModId>
    <Version>1.0.0</Version>
  </PropertyGroup>
</Project>
```

On every build, the SDK locates the game, selects the target framework (.NET 10 for IL2CPP games, .NET Standard 2.1 for
Mono games), references Lustral.Api, HarmonyX and the game's assemblies without copying them, validates the project,
generates the [manifest](./manifest) and deploys the mod to `<game>/Mods/<id>/`.

## Game

| Property | Description |
| --- | --- |
| `LustralGame` | The game's folder name under a Steam library's `steamapps\common` (for example `Lethal Company`), or the full path to the game folder. All Steam libraries on the machine are searched. |
| `LustralGameDir` | The full path to the game folder. Takes precedence over `LustralGame`. Set it in `Game.props.user` next to the project to keep machine-specific paths out of source control. |
| `LustralMonoGame`, `LustralIl2CppGame` | Used instead of `LustralGame` for [multi-backend mods](/developer-guide/multi-backend-mods): one game of each kind. The project is built once per backend (`netstandard2.1` for Mono, `net10.0` for IL2CPP), each against and into its game. |
| `LustralMonoGameDir`, `LustralIl2CppGameDir` | The full paths of those games, analogous to `LustralGameDir`. |
| `LustralBackend` | Set on the command line (`-p:LustralBackend=Mono`) to build only one backend of a multi-backend mod. |

The SDK defines the following properties for use in the project: `LustralBackend` (`Il2Cpp` or `Mono`),
`LustralGameDirectory`, `LustralModsDirectory` (the game's `Mods` folder), `LustralInteropDirectory` (IL2CPP) and
`LustralManagedDirectory` (Mono), as well as the compilation symbol `LUSTRAL_IL2CPP` or `LUSTRAL_MONO`.

## Mod Metadata

| Property | Default | Description |
| --- | --- | --- |
| `ModId` | Required | The mod ID: lowercase letters and digits, separated by single dots, dashes or underscores (`you.betterhud`). It names the mod's folder, settings file and log entries, and must not change. |
| `ModName` | Assembly name | The display name. |
| `Version` | `1.0.0` | The mod's [semantic version](./versions). |
| `Description` | | A one-sentence description of the mod. |
| `Authors` | | The authors, separated by semicolons. |
| `ModCredits` | | Acknowledgements, shown with the mod's details. |
| `PackageLicenseExpression` | | An SPDX license expression, such as `MIT`. |
| `PackageProjectUrl` | | The mod's website, written as its `website` link. |
| `RepositoryUrl` | | The mod's source repository, written as its `source` link. |
| `ModKind` | `mod` | `mod`, or `library` for a [library mod](/developer-guide/library-mods). |
| `ModColor` | Derived from the ID | The color of the mod's name in the console: a Unity color name (`orange`) or `#rrggbb`. |
| `ModLoadOrder` | `0` | Among mods that do not depend on each other, lower values load first. |

## Requirements

See [Compatibility Requirements](/developer-guide/compatibility).

| Property | Default | Description |
| --- | --- | --- |
| `ModLustralVersion` | `^<SDK version>` | The supported Lustral versions, as a [version range](./versions). |
| `ModBackends` | The game's backend | `mono`, `il2cpp`, or both: `mono;il2cpp`. |
| `ModPlatforms` | Any | `windows`, `linux` or `macos`, optionally followed by `-x64`, `-x86` or `-arm64`; separated by semicolons. |
| `ModUnityVersion` | Any | A [version range](./versions) of supported Unity versions, such as `>=2022.3`. |
| `ModProcesses` | Any | Executable names, for games that ship more than one executable. |

## Distribution and Updates

See [Publishing and Updates](/developer-guide/publishing).

| Property | Default | Description |
| --- | --- | --- |
| `ModUpdateSource` | | The update source: `github:owner/repository`, or the HTTPS address of a JSON feed. |
| `ModUpdateAsset` | The release's only `.zip` | For GitHub sources: the release file to download, with `*` wildcards. |
| `ModPackage` | `false` | `true` to also write `<id>-<version>.zip` to the output folder, containing the mod's folder, for attaching to a release. |

## Build and Launch

| Property | Default | Description |
| --- | --- | --- |
| `LustralDeploy` | `true` | `false` to build without deploying the mod to the game. |
| `LustralLaunchSettings` | `true` | `false` to prevent the SDK from writing `Properties/launchSettings.json`. |
| `LustralSteamAppId` | From the Steam library | The Steam app ID set by the launch profile, if the SDK cannot determine it. |
| `LustralMonoDebuggerAddress` | `127.0.0.1:56000` | The address of Mono's debugger agent, used by launch profiles for Mono games. |

## Items

```xml
<ItemGroup>
  <!-- Other mods -->
  <ModDependency Include="someone.uikit" Version="^1.2" />
  <ModDependency Include="someone.extras" Version="*" Optional="true" />
  <ModIncompatibility Include="someone.otherhud" Version="&lt;2.0" />

  <!-- Supported games -->
  <ModGame Include="BloonsTD6" Developer="Ninja Kiwi" Versions="56.*" />

  <!-- Additional links shown with the mod's details -->
  <ModLink Include="issues" Url="https://github.com/you/betterhud/issues" />

  <!-- Assemblies a library exposes to its dependents -->
  <ModExport Include="UiKit" />

  <!-- Files shipped with the mod -->
  <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

| Item | Metadata | Description |
| --- | --- | --- |
| `ModDependency` | `Version` (required), `Optional`, `Reference` | A required mod, or with `Optional="true"`, a mod used when present. If it is a library installed in the game, its exported assemblies are referenced (`Reference="false"` omits them). See [Mod Dependencies](/developer-guide/dependencies). |
| `ModIncompatibility` | `Version` (default: all) | A mod this mod is incompatible with: if it is loaded, this mod is not. |
| `ModGame` | `Developer`, `Versions` | A supported game, by the name the game reports. `Versions` are separated by semicolons and support `*` wildcards. |
| `ModLink` | `Url` | A named HTTP(S) link. |
| `ModExport` | | For libraries: an assembly name (without `.dll`) that dependents may use. Defaults to the library's own assembly. |
| Any item copied to the output | | Deployed to the mod's folder at the same relative path and included in the release package. See [Files and Data](/developer-guide/files-and-data). |

## Validation

The build fails with a descriptive error when:

- `ModId` is missing or invalid, or `ModKind` is neither `mod` nor `library`.
- The game cannot be found or is not a Unity game.
- The project sets a `TargetFramework` other than the one required by the game's backend.
- The game uses IL2CPP and has no interop assemblies yet (launch the game once with Lustral installed).
- A version, version range, color, link, platform, backend or update source is malformed.
