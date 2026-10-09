# Mod Lifecycle

Each mod contains one class deriving from [`Mod`](/api/mod), with a public parameterless constructor. Lustral locates it
in the mod's assembly, creates a single instance when the game starts and calls it on Unity's main thread.

```csharp
public sealed class ModEntry : Mod
{
    internal static ModEntry Instance { get; private set; } = null!;

    public ModEntry() => Instance = this;

    public override void OnLoad() { }
    public override void OnGameReady() { }
    public override void OnShutdown() { }
}
```

The build validates this: a missing `Mod` class, multiple `Mod` classes, or a class Lustral cannot instantiate is
reported as an error ([LUS0001 to LUS0003](/reference/analyzers)). [Library mods](./library-mods) do not have a mod
class.

## Lifecycle Methods

Mods are loaded in [load order](./dependencies#load-order); a mod's dependencies are always loaded first. Each mod's
constructor and `OnLoad` method run before the next mod is loaded.

### OnLoad

Called once, before the game loads its first scene and before most of the game's code has run. Use it to apply Harmony
patches (so that they are in place before the patched code runs), bind settings and subscribe to events.

Unity is not yet initialized: on IL2CPP games, mods are loaded while the runtime is still starting. Defer the use of
GameObjects, scenes, asset bundles and other Unity APIs to `OnGameReady`.

### OnGameReady

Called once, immediately after the game's first scene has loaded and before the [`SceneLoaded`](./game-events) event for
that scene. Unity's APIs are available from this point.

### OnShutdown

Called once when the game exits normally, in reverse load order. Keep the implementation short, for example saving data
and closing files. It is not called if the game crashes or is terminated.

### Constructor

All services are available in the constructor, so field initializers and the constructor can use `Log`, `Config` and the
other properties. Most mods only assign `Instance` in the constructor and perform initialization in `OnLoad`.

## Mod Services

| Property | Description |
| --- | --- |
| [`Info`](/api/mod-info) | The mod's ID, name, version, authors, description, folder (`Directory`) and data folder (`DataDirectory`). |
| [`Log`](./logging) | The mod's logger, tagged with its ID. |
| [`Harmony`](./harmony-patching) | A Harmony instance identified by the mod's ID. |
| [`Config`](./configuration) | The mod's settings file. |
| [`Events`](./game-events) | Scene events and per-frame events. |
| [`Coroutines`](./coroutines) | The mod's coroutines. |
| [`Assets`](./asset-bundles) | The mod's Unity asset bundles. |
| [`Game`](/api/game-info) | Information about the game: name, developer, version, Unity version, scripting backend and folder. |
| [`Mods`](./dependencies#checking-for-other-mods) | The mods loaded so far, in load order. |

Code outside the mod class accesses these services through the mod instance, which the template exposes as a static
`Instance` property: `ModEntry.Instance.Log.Info("...")`.

## Error Handling

Lustral isolates failures so that one mod cannot prevent others from loading:

- **The constructor or `OnLoad` throws an exception**: the mod fails to load. The error and stack trace are logged, any
  Harmony patches the mod applied are removed, and mods that depend on it are not loaded. All other mods load normally.
  If a Harmony patch targets a method that does not exist, the error names the patch class and the method.
- **`OnGameReady`, `OnShutdown`, an event handler or a coroutine throws an exception**: the error is logged under the
  mod's ID and execution continues. A coroutine that throws is stopped; an event handler remains subscribed.
- **An unhandled exception occurs on a thread started by the mod**: on IL2CPP games this terminates the game, as in any
  .NET application, and Lustral writes a crash report naming the mods on the thread's call stack. Handle exceptions in
  your own threads and tasks.

```txt
[12:00:01.234] [Error] [Core] you.betterhud failed to load
System.InvalidOperationException: ...
   at BetterHud.ModEntry.OnLoad() in C:\...\ModEntry.cs:line 21
```

## Threading

Lustral calls mods (lifecycle methods, event handlers and coroutines) on Unity's main thread, which is the only thread on
which Unity's APIs can be used. Background threads (`Task.Run` or custom threads) can be used for work that does not
involve Unity; return results to the main thread through a coroutine or an `Events.Update` handler.
