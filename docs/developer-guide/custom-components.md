# Custom Components

Most mods do not require their own components: [game events](./game-events) and [coroutines](./coroutines) cover
per-frame logic. Components are appropriate when behavior is tied to a GameObject, for example following an object,
reacting to its collisions, or receiving Unity messages (`OnTriggerEnter`, `OnDestroy`, `SendMessage`).

## Mono

On Mono games, a component is a standard MonoBehaviour:

```csharp
public class Spinner : MonoBehaviour
{
    public float Speed = 90;

    private void Update() => transform.Rotate(0, Speed * Time.deltaTime, 0);
}
```

```csharp
someGameObject.AddComponent<Spinner>();
```

## IL2CPP

On IL2CPP games, Unity only recognizes classes that exist in the game's native code. Lustral registers mod classes with
the IL2CPP runtime when the mod loads, using [Il2CppInterop](https://github.com/BepInEx/Il2CppInterop)'s injected types.
Such classes are declared as follows:

```csharp
using Il2CppInterop.Common.Attributes;
using UnityEngine;

[InjectedType]
public partial class Spinner : MonoBehaviour
{
    [Il2CppField]
    public partial Il2CppSystem.Single Speed { get; set; }

    [Il2CppMethod]
    private void Awake() => Speed = 90;

    [Il2CppMethod]
    private void Update() => transform.Rotate(0, Speed * Time.deltaTime, 0);
}
```

```csharp
someGameObject.AddComponent<Spinner>();
```

Requirements:

- **`[InjectedType]` and `partial`.** Il2CppInterop's source generator, included in the mod's references, generates the
  rest of the class, including the constructor IL2CPP requires and its registration. Lustral registers every
  `[InjectedType]` class in the mod when the mod loads.
- **`[Il2CppMethod]` on every method Unity calls**: `Awake`, `Start`, `Update`, `OnDestroy`, `OnTriggerEnter`, and
  methods invoked through `SendMessage`. Only these methods are visible to Unity. The build reports Unity messages
  without the attribute ([LUS0301](/reference/analyzers#lus0301)).
- **State stored in properties marked `[Il2CppField]` or `[ManagedField]`**, not in plain fields. The C# object is a
  wrapper around the native object, and the game may return a different wrapper for the same component later, so state
  must be stored on the native object:
  - `[Il2CppField]` for IL2CPP values: numbers and flags as `Il2CppSystem.Int32`, `Il2CppSystem.Single` or
    `Il2CppSystem.Boolean` (not `int`, `float` or `bool`), `Il2CppSystem.String`, and Unity or game types. These convert
    implicitly to and from .NET types, so expressions such as `Speed * Time.deltaTime` and `Hits + 1` work as expected.
    The build reports fields declared with a .NET type ([LUS0302](/reference/analyzers#lus0302)).
  - `[ManagedField]` for any .NET object, such as a `List<string>` or the mod's own classes.

```csharp
[InjectedType]
public partial class Tracker : MonoBehaviour
{
    [Il2CppField]
    public partial Il2CppSystem.Int32 Hits { get; set; }

    [ManagedField]
    public partial List<string> Log { get; set; }

    [Il2CppMethod]
    private void Awake() => Log = [];

    [Il2CppMethod]
    private void OnTriggerEnter(Collider other)
    {
        Hits = Hits + 1;
        Log.Add(other.name);
    }
}
```

::: warning Initialize [ManagedField] properties before reading them
In the current Il2CppInterop version, reading a `[ManagedField]` property that has never been assigned throws an
exception. Assign each one in `Awake`, as shown above. Declare its type without a nullable annotation (`?`), which the
source generator does not support.
:::

::: info Interfaces
Injected classes cannot yet implement the game's or Unity's interfaces, such as `IPointerClickHandler` and other UI
event handlers. Use Unity's `EventTrigger` component, or patch the game's own handler, in the meantime.
:::

## Supporting Both Backends

To support Mono and IL2CPP games from a single project, declare each form of the component under `#if LUSTRAL_IL2CPP`
and `#else`. See [Multi-Backend Mods](./multi-backend-mods).
