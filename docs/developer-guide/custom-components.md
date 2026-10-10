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
- **A top-level, non-generic class**, deriving directly from a non-generic type such as `MonoBehaviour`. Il2CppInterop
  cannot inject generic or nested types, types derived from a generic type, or types that implement a generic IL2CPP
  interface ([LUS0304](/reference/analyzers#lus0304)).
- **`[Il2CppMethod]` on every method Unity calls**: `Awake`, `Start`, `Update`, `OnDestroy`, `OnTriggerEnter`, and
  methods invoked through `SendMessage`. Only these methods are visible to Unity. The build reports Unity messages
  without the attribute ([LUS0301](/reference/analyzers#lus0301)). Generic methods cannot be exposed this way
  ([LUS0303](/reference/analyzers#lus0303)); they can still be called from the class's other methods.
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

### Interfaces

Injected classes can implement the game's and Unity's interfaces, such as `IPointerClickHandler` and the other UI event
handlers, and the game's code calls them as it calls its own classes. Mark each method that implements an interface
method with `[Il2CppMethod]`; without it, the class cannot be registered, and the build reports an error
([LUS0305](/reference/analyzers#lus0305)).

```csharp
using Il2CppInterop.Common.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[InjectedType]
public partial class ClickCounter : MonoBehaviour, IPointerClickHandler
{
    [Il2CppField]
    public partial Il2CppSystem.Int32 Clicks { get; set; }

    [Il2CppMethod]
    public void OnPointerClick(PointerEventData eventData) => Clicks = Clicks + 1;
}
```

As in any Unity project, a GameObject receives pointer events only if it has a raycast target, such as an `Image`, and the
scene has an `EventSystem`. Generic interfaces cannot be implemented ([LUS0304](/reference/analyzers#lus0304)).

## Supporting Both Backends

To support Mono and IL2CPP games from a single project, declare each form of the component under `#if LUSTRAL_IL2CPP`
and `#else`. See [Multi-Backend Mods](./multi-backend-mods).
