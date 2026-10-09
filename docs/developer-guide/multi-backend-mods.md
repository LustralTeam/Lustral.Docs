# Multi-Backend Mods

Some mods are not specific to one game, for example a free camera, a frame rate counter or a debug console. Such mods
can support every Unity game, Mono and IL2CPP alike, by meeting two conditions:

1. **No game requirement.** A mod without a [`ModGame`](./compatibility#game) entry loads in any game.
2. **A build for each backend**, produced from a single project as described below.

## Project Setup

Mono and IL2CPP mods are separate builds: Mono mods target .NET Standard 2.1 and compile against the game's assemblies,
while IL2CPP mods target .NET 10 and compile against [interop assemblies](./scripting-backends#il2cpp-games). A single
assembly cannot serve both, but a single project can produce both builds. Instead of `LustralGame`, specify one game of
each kind:

```xml
<PropertyGroup>
  <LustralMonoGame>PlagueInc</LustralMonoGame>
  <LustralIl2CppGame>BloonsTD6</LustralIl2CppGame>
  <ModId>you.fpscounter</ModId>
</PropertyGroup>
```

Alternatively, create the project from the template with both options:

```sh
dotnet new lustral-mod -n FpsCounter --mono-game PlagueInc --il2cpp-game BloonsTD6 --id you.fpscounter
```

The SDK builds the project once per backend from the same source:

| | Mono build | IL2CPP build |
| --- | --- | --- |
| Target framework | `netstandard2.1` | `net10.0` |
| Compiles against | The assemblies of `LustralMonoGame` | The interop assemblies of `LustralIl2CppGame` |
| Compilation symbol | `LUSTRAL_MONO` | `LUSTRAL_IL2CPP` |
| Output | `bin/<configuration>/netstandard2.1/` | `bin/<configuration>/net10.0/` |
| Deployed to | The `Mods` folder of `LustralMonoGame` | The `Mods` folder of `LustralIl2CppGame` |

Each build generates its own `mod.toml`, which declares the backend it targets. The two games are used for building and
testing; the resulting builds run in any other game of the same kind.

## Building and Debugging

- `dotnet build` builds both backends. `-p:LustralBackend=Mono` or `-p:LustralBackend=Il2Cpp` builds only one.
- In Visual Studio or Rider, building the project builds both backends. The editor's target framework selector
  determines which build IntelliSense uses (`netstandard2.1` for Mono, `net10.0` for IL2CPP), so both sides of an `#if`
  can be inspected.
- **F5** offers a launch profile for each game. [Debugging](./debugging) works as it does for single-backend mods.
- Machine-specific paths belong in `Game.props.user`, as `<LustralMonoGameDir>` and `<LustralIl2CppGameDir>`.
- `LustralGame` and `LustralGameDir` cannot be combined with these properties. A project that sets both, including a
  `LustralGameDir` left in `Game.props.user`, fails to build with an error explaining the conflict.
- `LustralGameStore` applies to both games; `LustralMonoGameStore` and `LustralIl2CppGameStore` set the store per game.

## Backend-Specific Code

Lustral's API is identical on both backends, as is most Unity code: `transform.position`, `GameObject.Find("Player")`,
`Time.deltaTime` and `new GameObject("My mod")` compile for both. On IL2CPP, strings and primitive types convert
implicitly (see [Mono and IL2CPP](./scripting-backends#strings-and-primitive-types)). Where the backends differ, use
conditional compilation:

```csharp
#if LUSTRAL_IL2CPP
[InjectedType]
public partial class FrameCounter : MonoBehaviour
{
    [Il2CppField]
    public partial Il2CppSystem.Int32 Frames { get; set; }

    [Il2CppMethod]
    private void Update() => Frames = Frames + 1;
}
#else
public class FrameCounter : MonoBehaviour
{
    private int _frames;

    private void Update() => _frames++;
}
#endif
```

Code most commonly differs in:

- [Custom components](./custom-components): plain classes on Mono, injected types on IL2CPP.
- Game collections: `List<T>` on Mono, `Il2CppSystem.Collections.Generic.List<T>` on IL2CPP.
- Delegates passed to the game: on IL2CPP, events are exposed as `add_` and `remove_` methods (see
  [Delegates](./scripting-backends#delegates)).

## Compatibility With Other Games

The mod is compiled against two specific games but runs in games built with other Unity versions and, on IL2CPP, with
different parts of Unity stripped from the build. If a game lacks a Unity member the mod uses, a
`MissingMethodException` or `TypeLoadException` is thrown when that code runs. Prefer commonly used Unity APIs; when
using less common ones, check for their presence first (through reflection or exception handling), and use
[`Game.UnityVersion`](/api/game-info#unityversion) and [`Game.Backend`](/api/game-info#backend) to determine the
environment.

These exceptions are thrown when the method containing the call is compiled, before any of its code runs, so a
`try` block in the same method does not catch them. Place the call in a separate method and handle the exception in
the caller:

```csharp
public override void OnGameReady()
{
    try
    {
        DrawOverlay();
    }
    catch (Exception e) when (e is MissingMethodException or TypeLoadException)
    {
        Log.Warn($"The overlay is not available in this game: {e.Message}");
    }
}

private static void DrawOverlay() => GUI.Label(new Rect(10, 10, 200, 25), "Hello");
```

Games also differ in their culture settings: some use the invariant culture, others the culture of the player's
system, in which the decimal separator may be a comma. Format and parse numbers in files and network data with
`CultureInfo.InvariantCulture`. Settings files written by Lustral are culture-independent.

## Publishing

With [`ModPackage`](./publishing#creating-a-release-package) enabled, a Release build produces one package per backend:

```txt
you.fpscounter package -> C:\...\bin\Release\you.fpscounter-1.0.0-mono.zip (SHA-256 ...)
you.fpscounter package -> C:\...\bin\Release\you.fpscounter-1.0.0-il2cpp.zip (SHA-256 ...)
```

Attach both packages to the GitHub release. Lustral's [automatic updates](./publishing#automatic-updates) select the
package matching the game's backend and never the other. Instruct users to download the package matching their game,
as with Lustral itself.
