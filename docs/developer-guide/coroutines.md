# Coroutines

A coroutine is a method that executes in steps, waiting between them: for a duration, a frame, or until a condition is
met. [`Coroutines`](/api/coroutines) runs coroutines for the mod, identically on Mono and IL2CPP games and without
requiring a MonoBehaviour.

```csharp
public override void OnGameReady() => Coroutines.Start(Welcome());

private IEnumerator Welcome()
{
    yield return Wait.Seconds(3);
    Log.Info("Three seconds have passed");

    yield return Wait.Until(() => PlayerIsInLevel());
    Log.Info("The player has entered a level");
}
```

A coroutine is an iterator method returning `IEnumerator` (from `System.Collections`). Each `yield return` specifies
how long to wait before the next step. All steps run on Unity's main thread.

## Wait Instructions

| `yield return` | Resumes |
| --- | --- |
| `null` or `Wait.NextFrame` | On the next frame, after `Update`. |
| `Wait.Frames(n)` | After `n` frames (1 is the next frame). |
| `Wait.Seconds(s)` | After `s` seconds of game time, which follows the game's time scale, like Unity's `WaitForSeconds`. |
| `Wait.RealSeconds(s)` | After `s` seconds of real time, regardless of the time scale. |
| `Wait.Until(() => ...)` | When the condition becomes true (checked immediately, then once per frame). |
| `Wait.While(() => ...)` | When the condition becomes false. |
| `Wait.FixedUpdate` | At the next physics step. |
| `Wait.LateUpdate` | After the game's `LateUpdate` in the current frame (or the next frame, if it has already passed). |
| Another `IEnumerator` | When the nested coroutine completes. |
| A [`CoroutineHandle`](/api/coroutine-handle) | When the referenced coroutine ends. |
| A `Task` | When the task completes, allowing work on other threads without blocking the game. |
| Unity's `WaitForSeconds`, `WaitForFixedUpdate`, `WaitForEndOfFrame`, `WaitUntil`, `WaitWhile`, any `CustomYieldInstruction`, an `AsyncOperation` | As in Unity. `WaitForEndOfFrame` resumes after `LateUpdate`. |

Prefer `Wait` over Unity's yield instructions: it is available on every game, whereas an IL2CPP build may have stripped
the constructor of a Unity instruction the game does not use.

## Starting and Stopping Coroutines

`Coroutines.Start` returns a [`CoroutineHandle`](/api/coroutine-handle):

```csharp
var spinner = Coroutines.Start(Spin());
// ...
Coroutines.Stop(spinner);          // stops it before its next step
if (spinner.IsRunning) { }         // false once it has ended, stopped or failed
Coroutines.StopAll();              // stops every coroutine started by the mod
```

When called on the main thread, `Start` runs the coroutine up to its first `yield` immediately, like Unity's
`StartCoroutine`. When called from another thread, the first step runs on the main thread in the next frame, which makes
coroutines a convenient way to return to the main thread:

```csharp
_ = Task.Run(async () =>
{
    var news = await DownloadNewsAsync();
    Coroutines.Start(Show(news));   // Show can safely call Unity APIs
});
```

Coroutines are not bound to a GameObject or scene; they continue across scene loads until they complete or are stopped.

## Exceptions

A coroutine that throws an exception is stopped, and the error is logged under the mod's ID with the coroutine's name.
Other coroutines continue to run.

## Awaiting Tasks

```csharp
private static readonly HttpClient Http = new();

private IEnumerator LoadProfile(string url)
{
    var download = Http.GetStringAsync(url);
    yield return download;                     // the game continues running while the task completes
    if (download.IsFaulted)
        Log.Error("Could not load the profile", download.Exception);
    else
        Apply(download.Result);
}
```
