# Installation Guide

Lustral is installed separately in each game.

## Step 1: Locate the Game Folder

The game folder is the folder that contains the game's executable (`.exe`).

- **Steam:** right-click the game in your library and select **Manage > Browse local files**.
- **Epic Games Launcher:** in your library, click the three dots next to the game, select **Manage**, and open the
  folder shown under **Installation**.
- **Other sources:** the folder you chose when installing the game.

## Step 2: Download the Correct Package

Lustral provides a package for each scripting backend. Check the game folder to determine which one to download:

| The game folder contains | Scripting backend | Package |
| --- | --- | --- |
| `GameAssembly.dll` | IL2CPP | `lustral-win-x64-il2cpp.zip` |
| A `<game>_Data/Managed` folder with DLL files | Mono | `lustral-win-x64-mono.zip` |

The buttons below download the latest version:

<DownloadButtons />

Lustral supports 64-bit games. Earlier versions and the release notes are available on
[GitHub](https://github.com/LustralTeam/Lustral/releases).

## Step 3: Extract the Package

Extract the archive directly into the game folder, so that `winhttp.dll` and the `Lustral` folder are next to the
game's executable:

```txt
<game>/
├─ <game>.exe
├─ <game>_Data/
├─ winhttp.dll
└─ Lustral/
```

If extraction creates a `lustral-win-x64-il2cpp` or `lustral-win-x64-mono` subfolder instead, move its contents up into
the game folder.

## Step 4: Launch the Game

Launch the game as usual. On its first start, Lustral creates:

- `Mods/`: the folder for installed mods.
- `Lustral/loader.toml`: Lustral's [configuration](./configuration) for this game.
- `Lustral/Logs/latest.log`: the log file.

**IL2CPP games:** on the first launch, a window titled *Preparing \<game\> for mods* appears before the game starts.
Lustral analyzes the game's code to make it available to mods. This typically takes one to two minutes and is repeated
only after the game is updated. Selecting **Play without mods** starts the game immediately without mods; Lustral
resumes preparation on the next launch.

## Verifying the Installation

Open `Lustral/Logs/latest.log`. The first lines identify Lustral and the game:

```txt
[Info ] [Lustral] Lustral 0.1.0 on .NET 10.0.12, X64
[Info ] [Lustral] Game: BloonsTD6 57.0 by Ninja Kiwi, Unity 6000.3.23f1 (C:\...\BloonsTD6.exe)
```

If no `Lustral/Logs` folder exists after launching the game, Lustral did not start. See
[Troubleshooting](./troubleshooting#lustral-does-not-start).

::: tip Live log output
Set `console = true` in the `[log]` section of `Lustral/loader.toml` to show the log in a console window while the game
runs.
:::

Once Lustral is installed, continue with [Managing Mods](./managing-mods).

## Game Updates

Game updates do not affect Lustral or installed mods. IL2CPP games repeat the preparation step on the next launch.
Mods built for a previous game version may not load until their authors update them; the log records the reason.

## Updating Lustral

1. Download the package for the new version.
2. Delete the existing program files:
   - `winhttp.dll`
   - `Lustral/core`
   - `Lustral/dotnet` and `Lustral/generator` (IL2CPP games only)
3. Extract the new package as described in [Step 3](#step-3-extract-the-package).

The remaining contents of the `Lustral` folder, including settings, mod settings, mod data and logs, are preserved.

## Uninstalling

Delete `winhttp.dll` and the `Lustral` folder from the game folder. Optionally, delete the `Mods` folder to remove all
installed mods.

To keep your settings for a later reinstallation, keep `Lustral/loader.toml` and `Lustral/config`. To run the game
without mods temporarily, see [Running Without Mods](./troubleshooting#running-without-mods).
