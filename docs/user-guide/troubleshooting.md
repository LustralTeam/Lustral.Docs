# Troubleshooting

Most problems can be diagnosed from the log file, `Lustral/Logs/latest.log` in the game folder. Lustral records its
actions there, along with the reason for any failure. Logs from the ten previous sessions are kept in the same folder.

::: tip
Set `console = true` in the `[log]` section of `Lustral/loader.toml` to view the log in a console window while the game
runs.
:::

## Lustral Does Not Start

The game runs, but the `Lustral/Logs` folder is missing or contains no new log.

- **File placement:** `winhttp.dll` and the `Lustral` folder must be located directly next to the game's executable,
  not in a subfolder. See the [Installation Guide](./installation#step-3-extract-the-package).
- **Package type:** games containing `GameAssembly.dll` require the `il2cpp` package; all others require the `mono`
  package. With the wrong package, Lustral shows a message that names the package to install, and the game starts
  without mods.
- **64-bit games only:** Lustral supports 64-bit games. Windows does not load Lustral's `winhttp.dll` into a 32-bit
  game, so the game runs without mods and no log is written. The Task Manager shows "(32 bit)" after the name of a
  32-bit game.
- **Other mod loaders:** Lustral cannot be installed alongside MelonLoader or BepInEx. BepInEx uses its own
  `winhttp.dll`, so installing one replaces the other. Uninstall the other loader and reinstall Lustral.
- **Antivirus software:** check whether `winhttp.dll` or files in the `Lustral` folder were removed or blocked.

If `Lustral/Logs/stage0.log` exists, its last entries show how far the startup progressed. A message that the game
"uses Unity's legacy .NET 3.5 scripting runtime" means that the game cannot run Lustral: some games made with Unity 2018
use this older runtime, which Lustral does not support. The game runs normally without mods; delete `winhttp.dll` from
the game folder to remove Lustral.

## A Mod Does Not Load

Search the log for the mod's ID. A `Not loading` entry states why the mod was skipped (see
[Mods That Fail to Load](./managing-mods#mods-that-fail-to-load)). An error with a stack trace indicates a failure in
the mod itself, which should be reported to its author.

Lustral also reports mods installed in the wrong location, such as an archive that was not extracted, a folder nested
one level too deep, or a DLL placed directly in `Mods`. If the log does not mention the mod at all, Lustral did not find
it: its manifest must be located at `Mods/<mod id>/mod.toml`.

## The Game Crashes

After a crash, Lustral writes a crash report on the next launch:

```txt
[Warn ] [Crash] The last session crashed (the crash records mention someone.betterhud). Crash report: C:\...\Lustral\Reports\crash-2026-10-08_14-03-12.txt
```

The report names the mods involved when they can be identified. Disable those mods (see below), confirm that the game
runs, and send the report to the mod's author.

If no mod is named, identify the cause by elimination: disable half of the installed mods, check whether the crash
persists, and repeat with the half that contains the problem.

## Running Without Mods

- **Without specific mods:** add them to `disabled` in `Lustral/loader.toml` (see
  [Disabling a Mod](./managing-mods#disabling-a-mod)).
- **Without Lustral, through the launch options:** add `--no-mods` to the game's launch options (the same option as in
  MelonLoader). In Steam, open the game's **Properties** and enter it under **Launch Options**; other launchers have a
  similar setting. The game then starts without Lustral until the option is removed.
- **Without Lustral, through its files:** rename `winhttp.dll` in the game folder, for example to
  `winhttp.dll.disabled`. The game then starts without Lustral. Restore the original name to enable it again.

## IL2CPP Game Preparation

On the first launch, and after each game update, Lustral displays *Preparing \<game\> for mods* while it analyzes the
game's code. This is expected and typically takes one to two minutes. **Play without mods** skips the step for the
current session.

If preparation fails, a window titled *Could not prepare \<game\> for mods* is shown and the game starts without mods.
Lustral retries on the next launch. If the problem persists, report it as described below, including the log file named
in the window.

## Reporting Issues

- **Questions** about installing Lustral or using mods: ask in the `#user-support` forum on the
  [Lustral Discord server](https://discord.gg/3Y6Ete73E3).
- **Problems with a mod** (errors, incorrect behavior, crashes caused by the mod): contact the mod's author.
- **Problems with Lustral** (Lustral does not start, game preparation fails, behavior differs from this documentation):
  open an issue on [GitHub](https://github.com/LustralTeam/Lustral/issues).

Include `Lustral/Logs/latest.log` from an affected session, the crash report if one exists, and the list of installed
mods. Note that log files contain file paths, which may include your Windows user name.

Unity also writes its own log, `Player.log`, to `%USERPROFILE%\AppData\LocalLow\<developer>\<game>\` (some games disable
it). It can provide additional information about crashes.
