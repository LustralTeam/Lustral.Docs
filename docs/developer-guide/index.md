# Developer Guide

Lustral is a mod loader for Unity games. It starts with the game, loads the mods in the game's `Mods` folder, and
provides each mod with an API for common tasks: logging, patching game code, configuration, per-frame events,
coroutines and asset bundles.

Lustral supports both Unity scripting backends:

- **Mono** games ship their C# code as .NET assemblies (`<game>_Data/Managed`). Mods reference these assemblies directly
  and run in the game's Mono runtime.
- **IL2CPP** games compile their C# code to native code (`GameAssembly.dll`). Lustral generates .NET interop assemblies
  that mirror the game's types (using [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop)), and mods run on
  .NET 10 alongside the game.

Mods use the same `Lustral.Mod` base class on both backends. The differences lie in how mods access the game's own types,
as described in [Mono and IL2CPP](./scripting-backends).

This guide is intended for mod developers. For installing and using mods, see the [User Guide](/user-guide/).

## Features

- **SDK** (`Lustral.Sdk`): specify the game in the project file and build. The SDK locates the game, selects the target
  framework, references the game's assemblies, generates the mod manifest and deploys the mod. Debugging (F5) launches
  the game with the debugger attached.
- **Mod lifecycle**: a single base class with [`OnLoad`](./mod-lifecycle#onload),
  [`OnGameReady`](./mod-lifecycle#ongameready) and [`OnShutdown`](./mod-lifecycle#onshutdown).
- **HarmonyX patching**: a Harmony instance per mod, with analyzers that validate patch targets at compile time.
- **Configuration**: settings defined as a C# class and stored in a documented TOML file.
- **Events, coroutines and asset bundles**: identical behavior on Mono and IL2CPP, including on IL2CPP games whose build
  stripped the corresponding Unity APIs.
- **Dependencies**: version ranges, optional dependencies, incompatibilities and shared library mods.
- **Automatic updates**: mods can be updated from GitHub releases on the user's next launch.
- **Diagnostics**: log files, an optional color console, descriptive messages when a mod cannot load, and crash reports
  that identify the mods involved.

## System Requirements

- Windows, x64. Linux support is planned.
- Mono games on Unity 2019.4 and later; IL2CPP games on Unity 2022.3 and Unity 6. Earlier IL2CPP versions have not been
  tested.

::: info Pre-release
Lustral has not been publicly released yet. This documentation describes its first release; until then, the packages it
refers to (`Lustral.Sdk`, `Lustral.Templates`) are not available on NuGet.
:::

## Getting Help

Ask questions about mod development in the `#developer-support` forum on the [Lustral Discord server](https://discord.gg/3Y6Ete73E3), where
mods made with Lustral can also be shared in `#mod-showcase`. Report bugs in Lustral, its SDK or this documentation on
[GitHub](https://github.com/LustralTeam/Lustral/issues).

## Migrating from MelonLoader or BepInEx

| MelonLoader / BepInEx | Lustral |
| --- | --- |
| `MelonMod` / `BaseUnityPlugin` | [`Mod`](/api/mod) |
| `OnInitializeMelon` / `Awake` | [`OnLoad`](./mod-lifecycle#onload) |
| `OnSceneWasLoaded` | [`Events.SceneLoaded`](./game-events) |
| `OnUpdate` / `Update` | [`Events.Update`](./game-events) |
| `MelonPreferences` / `Config.Bind` | [`Config.Bind<Settings>()`](./configuration) |
| `MelonCoroutines.Start` | [`Coroutines.Start`](./coroutines) |
| `[MelonInfo]`, `[MelonGame]`, `[BepInPlugin]` | Project properties, written to [`mod.toml`](/reference/manifest) |
| `ClassInjector.RegisterTypeInIl2Cpp` | [`[InjectedType]`](./custom-components#il2cpp) |

Lustral does not run MelonLoader or BepInEx mods; mods must be built for Lustral.
