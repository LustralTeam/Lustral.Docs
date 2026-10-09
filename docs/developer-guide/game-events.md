# Game Events

[`Events`](/api/game-events) notifies mods of scene changes and provides per-frame callbacks, without requiring a
MonoBehaviour. Subscribe in `OnLoad` or whenever the events are needed, and unsubscribe when they are no longer needed.

```csharp
public override void OnLoad()
{
    Events.SceneLoaded += OnSceneLoaded;
    Events.Update += OnUpdate;
}

private void OnSceneLoaded(SceneInfo scene)
{
    if (scene.Name == "MainMenu")
        Log.Info("Returned to the main menu");
}

private void OnUpdate()
{
    // Called once per frame.
}
```

## Scene Events

| Event | Raised |
| --- | --- |
| `SceneLoaded` | After a scene has loaded, including the first scene, immediately after [`OnGameReady`](./mod-lifecycle#ongameready). |
| `SceneUnloaded` | After a scene has been unloaded. |

Both events provide a [`SceneInfo`](/api/scene-info) containing the scene's `Name` and `BuildIndex` (its index in the
game's build settings, or -1 for scenes loaded from asset bundles). `SceneInfo` is a plain struct, identical on Mono and
IL2CPP; its `ToString()` returns `Name (#BuildIndex)`.

## Frame Events

| Event | Raised | Typical use |
| --- | --- | --- |
| `Update` | Once per frame, after the game's `Update` methods. | Most per-frame logic: input, timers, UI state. |
| `LateUpdate` | Once per frame, after the game's `LateUpdate` methods. | Camera logic and anything that depends on the frame's movement. |
| `FixedUpdate` | Once per physics step, alongside the game's `FixedUpdate` methods and before the physics simulation. The rate is fixed (50 per second by default), so it may run zero or several times per frame. | Physics-related logic. |
| `OnGUI` | Several times per frame, once per GUI event, like `MonoBehaviour.OnGUI`. | Drawing with Unity's immediate-mode GUI (`GUI`, `GUILayout`). |

Frame events have no cost until a mod subscribes: Lustral only hooks into Unity's frame loop for events that have
subscribers. Subscribe only while there is work to do:

```csharp
private float _shownFor;

private void ShowBanner()
{
    _shownFor = 0;
    Events.OnGUI += DrawBanner;
    Events.Update += CountDown;
}

private void CountDown()
{
    _shownFor += UnityEngine.Time.deltaTime;
    if (_shownFor < 5)
        return;
    Events.OnGUI -= DrawBanner;
    Events.Update -= CountDown;
}

private void DrawBanner() => UnityEngine.GUI.Label(new UnityEngine.Rect(10, 10, 400, 30), "Saved!");
```

For logic spread across frames with waits in between, a [coroutine](./coroutines) is usually clearer than a handler that
counts frames.

## Exceptions in Handlers

Exceptions thrown by a handler are logged under the mod's ID; the handler remains subscribed, and other handlers and
mods continue to run. An exception thrown every frame is logged once, followed by a repetition count.

## OnGUI on IL2CPP Games

Some games' builds exclude Unity's immediate-mode GUI entirely; in that case, Lustral logs a message and `OnGUI` is
never raised. IL2CPP builds also only include the GUI methods the game itself uses, so verify that the methods you need
(`GUI.Label`, `GUILayout.Button`, ...) exist in the game's interop assemblies
(`Lustral/interop/UnityEngine.IMGUIModule.dll`).
