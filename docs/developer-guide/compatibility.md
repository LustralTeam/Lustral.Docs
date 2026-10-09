# Compatibility Requirements

A mod can declare the environments it supports: the game and game versions, the Unity version, the scripting backend,
the platform and the Lustral version. Lustral checks these requirements before loading the mod. If a requirement is not
met, the mod is skipped and the log states the reason, instead of the mod failing unpredictably:

```txt
[Warn ] [Core] Not loading you.fastforward: is for BloonsTD6 55.*; the installed version is 56.3
```

All requirements are optional. They are set in the project file, and the SDK writes them to the mod's manifest.

## Game

```xml
<ItemGroup>
  <ModGame Include="BloonsTD6" Developer="Ninja Kiwi" Versions="56.*" />
</ItemGroup>
```

- `Include` is the game's name as defined by the game itself (Unity's product name, stored in `<game>_Data/app.info`),
  which may differ from its store name: Bloons TD 6 identifies itself as `BloonsTD6`. Lustral's log reports the name,
  version and developer at startup:

  ```txt
  [Info ] [Lustral] Game: BloonsTD6 57.0 by Ninja Kiwi, Unity 6000.3.23f1 (C:\...\BloonsTD6.exe)
  ```

- `Developer` (optional) is Unity's company name, also stored in `app.info`.
- `Versions` (optional) lists the supported game versions, as reported by the game (Unity's `Application.version`),
  separated by semicolons. `*` is a wildcard: `56.*`, `1.2.*;1.3.*`.

A mod for multiple games lists each of them. A mod without a `ModGame` entry loads in any game, which is appropriate for
general-purpose mods such as a free camera or a frame rate counter.

If Lustral cannot determine a game's version, the version requirement is not enforced; the log notes that the mod was
loaded without the check.

## Unity Version, Backend, Platform and Process

| Property | Example | Default |
| --- | --- | --- |
| `ModUnityVersion` | `>=2022.3` | Any |
| `ModBackends` | `mono;il2cpp` | The backend of the game the mod is built against |
| `ModPlatforms` | `windows`, `windows-x64`, `linux` | Any |
| `ModProcesses` | `Game.exe;GameServer.exe`, for games with multiple executables | Any |

`ModUnityVersion` is a [version range](/reference/versions). `ModBackends` is relevant because a mod built for IL2CPP
cannot run on Mono and vice versa; the default value already reflects this.

## Lustral Version

```xml
<ModLustralVersion>^0.3</ModLustralVersion>
```

By default, a mod requires the Lustral version it was built with, or a newer version without breaking changes: the same
major version, or, before 1.0, the same minor version. This is appropriate for nearly all mods. Set it explicitly only
if the mod is known to work with an older Lustral version, or requires a newer version than the SDK it is built with.

## Behavior for Users

If a requirement is not met, the mod is not loaded and the log states the reason:

| Condition | Message |
| --- | --- |
| Different game | `is for BloonsTD6; the running game is PEAK` |
| Unsupported game version | `is for BloonsTD6 55.*; the installed version is 56.3` |
| Unsupported Unity version | `requires Unity >=6000.0; the game uses Unity 2022.3.62f2` |
| Different scripting backend | `is built for IL2CPP games; this game uses Mono` |
| Newer Lustral version required | `requires Lustral ^0.4; the installed version is 0.3.1` |

Users can override the checks by forcing the mod in `Lustral/loader.toml`:

```toml
[mods]
force = ["you.fastforward"]
```

A forced mod is loaded regardless of its requirements and incompatibilities, with a warning in the log. It may not work
correctly; forcing is intended for advanced users.
