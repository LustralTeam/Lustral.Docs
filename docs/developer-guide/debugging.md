# Debugging

## Launching From the IDE

On the first build, the SDK writes a launch profile (`Properties/launchSettings.json`), so that running the project in
Visual Studio, Rider or Visual Studio Code builds and deploys the mod and starts the game. For Steam games, the profile
also sets `SteamAppId`; without it, a Steam game started outside Steam restarts itself through Steam, detaching the
debugger.

The profile contains the local game path, so the template's `.gitignore` excludes it from source control. Delete the
file to have the SDK regenerate it, or set `<LustralLaunchSettings>false</LustralLaunchSettings>` to disable it.

## IL2CPP Games

On IL2CPP games, mods run on .NET 10, so F5 attaches the IDE's standard .NET debugger. Breakpoints, stepping, watches
and exception handling work in mod code, including Harmony patches. Breakpoints in `OnLoad` are also hit, since the
debugger attaches before Lustral loads mods.

## Mono Games

On Mono games, mods run in Unity's Mono runtime, which uses its own debugging protocol, so the IDE's .NET debugger cannot
attach. Instead, the launch profile enables Mono's debugger agent, which Lustral starts on `127.0.0.1:56000`. Start the
game with the profile without debugging (**Ctrl+F5** in Visual Studio), then attach a Unity debugger to that address:

- **Visual Studio** (with the *Game development with Unity* workload): select Debug > Attach Unity Debugger and enter
  `127.0.0.1` and port `56000`.
- **Rider**, **Visual Studio Code** with the Unity extension, or **dnSpy**: attach the Unity debugger to
  `127.0.0.1:56000`.

To debug `OnLoad`, use the second profile, **"(wait for debugger)"**: the game pauses at startup until a debugger
attaches, before any mod is loaded.

To use a different port, set `<LustralMonoDebuggerAddress>127.0.0.1:56001</LustralMonoDebuggerAddress>` and delete the
launch profile so that the SDK regenerates it.

## Logs

- `Lustral/Logs/latest.log`: Lustral's log, including mod output. Enable the console window (`console = true` in the
  `[log]` section of `Lustral/loader.toml`) to view it live, and set `level = "debug"` to include `Log.Debug` entries.
- Unity's log, `Player.log`, in `%USERPROFILE%\AppData\LocalLow\<developer>\<game>`: Unity's errors, including asset
  bundle loading failures. Some games disable it.

## Crash Reports

On the next launch after a session that ended without the game exiting normally, Lustral writes a crash report to
`Lustral/Reports/crash-<time>.txt` if there is evidence of a crash (Unity's crash output, a Windows crash dump or the
Windows event log). The report contains:

- The Lustral and game versions, the scripting backend, and the session's mods with their versions.
- **The mods named in the crash output**, whose code was on the crashing call stack.
- The crash output and the last part of the previous log.
- The methods each mod patched with Harmony.

On IL2CPP games, an unhandled exception on a mod's thread terminates the game immediately; Lustral reports it at once,
naming the mods whose code was on that thread's call stack.

A game terminated externally (for example from Task Manager) leaves no crash evidence; the next log contains a single
informational entry.

## Common Issues

| Symptom | Check |
| --- | --- |
| The mod does not appear in the log | The mod is in `Mods/<id>/` with its `mod.toml`; the build reported a deployment; Lustral is installed (`winhttp.dll` and `Lustral/` next to the game's executable). |
| `Not loading <id>: ...` | The reason after the colon: an unmet requirement, a missing dependency, or `disabled in loader.toml`. |
| `<id> failed to load` | The exception and stack trace that follow: the constructor or `OnLoad` threw an exception. |
| A patch never runs | `Harmony.PatchAll` is called in `OnLoad`. On IL2CPP, the target method may be inlined into its callers; patch the callers instead. |
| `MissingMethodException` or `TypeLoadException` after a game update | The game's code changed; rebuild the mod against the new version. |
