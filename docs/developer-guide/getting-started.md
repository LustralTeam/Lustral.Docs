# Getting Started

This page covers the one-time setup: installing Lustral in a game, installing the mod template, and building a first mod
that loads in the game.

## Prerequisites

- **Windows** (x64).
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**. It builds mods for both scripting backends:
  IL2CPP mods target .NET 10, Mono mods target .NET Standard 2.1.
- **An IDE**: Visual Studio 2022 (17.14 or later) or 2026, JetBrains Rider, or Visual Studio Code with the C# Dev Kit.
- **The game**, installed locally. Steam games are located by their folder name; other games by their path.

## Step 1: Install Lustral in the Game

Download the [Lustral release](https://github.com/LustralTeam/Lustral/releases) that matches the game's scripting
backend, as described in the [Installation Guide](/user-guide/installation):

- **IL2CPP** if the game folder contains `GameAssembly.dll`: `lustral-<version>-win-x64-il2cpp.zip`.
- **Mono** otherwise: `lustral-<version>-win-x64-mono.zip`.

Extract the archive into the game folder, so that `winhttp.dll` and the `Lustral` folder are next to the game's
executable.

Launch the game once. Lustral creates its configuration (`Lustral/loader.toml`) and log (`Lustral/Logs/latest.log`).
On IL2CPP games, the first launch also generates the game's **interop assemblies**, the .NET representation of the
game's code that mods compile against. A progress window is shown during generation, which typically takes one to two
minutes and is repeated only after the game is updated.

::: tip Live log output
Set `console = true` in the `[log]` section of `Lustral/loader.toml` to show the log in a console window.
:::

## Step 2: Install the Mod Template

The mod template is distributed as the NuGet package `Lustral.Templates`. Install it in one of the following ways:

- **Command line** (for all IDEs):

  ```sh
  dotnet new install Lustral.Templates
  ```

- **JetBrains Rider**: download `Lustral.Templates.<version>.nupkg` from the
  [release page](https://github.com/LustralTeam/Lustral/releases). In the **New Solution** dialog, click
  **More Templates**, then **Install Template**, select the downloaded file and click **Reload**.

Visual Studio has no option to install templates; use the command line. Templates installed from the command line are
also available in Visual Studio and Rider. To update an installed template to the latest version, run
`dotnet new update`.

## Step 3: Create a Mod Project

**In an IDE**, create a new project and select the **Lustral Mod** template (search for "Lustral"). Enter the game and
the mod ID in the template's options. If your IDE does not show these options, set `LustralGame` and `ModId` in the
project file after creating the project (see [Creating Your First Mod](./first-mod)).

**From the command line:**

```sh
dotnet new lustral-mod -n BetterHud --game "Lethal Company" --id you.betterhud
```

| Option | Description |
| --- | --- |
| `-n` | The project name, also used as the mod's display name. |
| `--game` | The game's folder name under a Steam library's `steamapps\common`, or the full path to the game folder. |
| `--id` | The mod ID: lowercase letters and digits, separated by dots, dashes or underscores. Prefix it with your name to avoid conflicts. Defaults to the project name in lowercase. |
| `--mono-game`, `--il2cpp-game` | Instead of `--game`, for a mod that targets both backends (see [Multi-Backend Mods](./multi-backend-mods)). |

The project contains two files, `BetterHud.csproj` and `ModEntry.cs`, described in
[Creating Your First Mod](./first-mod).

## Step 4: Build

```sh
cd BetterHud
dotnet build
```

The build compiles the mod against the game, generates its manifest and deploys it to the game:

```txt
you.betterhud -> C:\...\steamapps\common\Lethal Company\Mods\you.betterhud\
```

Close the game before building; Windows prevents the build from replacing files that are in use.

## Step 5: Run

Launch the game through Steam, or press **F5** in the IDE to launch it with the debugger attached (see
[Debugging](./debugging)). The log then contains:

```txt
[12:00:01.234] [Info ] [Core] Loaded BetterHud 0.1.0 (you.betterhud)
[12:00:01.240] [Info ] [you.betterhud] BetterHud 0.1.0 loaded in Lethal Company ...
```

The second line is written by the template's code and reports the game name and version.

Continue with [Creating Your First Mod](./first-mod) to add functionality.

## Games Outside Steam

Specify the full path with `--game "D:\Games\Some Game"`. To keep a machine-specific path out of the project file,
create `Game.props.user` next to the project instead (the template's `.gitignore` excludes it from source control):

```xml
<Project>
  <PropertyGroup>
    <LustralGameDir>D:\Games\Some Game</LustralGameDir>
  </PropertyGroup>
</Project>
```

## Build Errors

The SDK's error messages describe the required change. Common errors:

| Error | Resolution |
| --- | --- |
| Game not found: *game* | Check the folder name under `steamapps\common`, or specify the full path or use `Game.props.user`. |
| *Game* has no interop assemblies | The game uses IL2CPP and Lustral has not run in it yet. Install Lustral and launch the game once. |
| Could not deploy *id* to *game* | Close the game and build again. |
| Lustral is not installed in *folder* (warning) | The mod was built but cannot be loaded. Install Lustral in the game. |
