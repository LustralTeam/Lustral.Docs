# Asset Bundles

Models, textures, audio, prefabs and UI created in Unity are delivered to the game in **asset bundles**: files built by
the Unity editor and loaded at runtime. [`Assets`](/api/mod-assets) loads the bundles a mod ships, identically on Mono
and IL2CPP games.

```csharp
public override void OnGameReady()
{
    var bundle = Assets.LoadBundle("Assets/ui.bundle");
    var panel = bundle.Load<GameObject>("Panel");
    var icon = bundle.Load<Sprite>("icon");
}
```

## Building a Bundle

Asset bundles are built in the Unity editor:

1. **Use the game's Unity version**, or a version close to it. Unity cannot load bundles built with a newer version; a
   bundle built with an older version usually loads. The game's Unity version is shown in Lustral's log
   (`Game: ... Unity 2022.3.62f2`) and available as [`Game.UnityVersion`](/api/game-info#unityversion).
2. Keep Unity's **AssetBundle module** enabled in the project (Window > Package Manager > Built-in > AssetBundle; enabled
   by default in new projects). Bundles built without it cannot be loaded by any game.
3. Assign an asset bundle name to the assets (the AssetBundle field at the bottom of the Inspector) and build them with
   an editor script:

```csharp
using UnityEditor;

public static class BuildBundles
{
    [MenuItem("Assets/Build asset bundles")]
    public static void Build() =>
        BuildPipeline.BuildAssetBundles("Bundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
}
```

Asset bundles cannot contain scripts. Prefabs can use Unity's built-in components and the game's components (if the
Unity project contains the same scripts), but not the mod's own code; add the mod's components after loading (see
[Custom Components](./custom-components)).

## Including a Bundle in the Mod

Add the bundle to the project, for example in an `Assets` folder, and include it in the build output:

```xml
<ItemGroup>
  <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

The build copies it to `Mods/<id>/Assets/ui.bundle` in the game, and it is included in the mod's
[release package](./publishing). `LoadBundle` accepts a path relative to the mod's folder.

To distribute a single DLL instead, embed the bundle (`<EmbeddedResource Include="ui.bundle" />`) and load it from
memory:

```csharp
using var stream = typeof(ModEntry).Assembly.GetManifestResourceStream("BetterHud.ui.bundle")!;
using var bytes = new MemoryStream();
stream.CopyTo(bytes);
var bundle = Assets.LoadBundle(bytes.ToArray());
```

## Loading Assets

Assets in a bundle are identified by their full path in the Unity project, in lowercase, such as
`assets/ui/panel.prefab`. [`Load<T>`](/api/mod-asset-bundle#load) accepts either that path or the file name without
its extension:

```csharp
bundle.Load<GameObject>("Panel");                          // a prefab
bundle.Load<GameObject>("assets/ui/panel.prefab");         // the same prefab, by path
bundle.Load<Texture2D>("icon");                            // an image as a texture
bundle.Load<Sprite>("icon");                               // the same image as a sprite
bundle.Load<AudioClip>("click");
```

`T` is the Unity type to load the asset as. If the bundle contains no asset with that name and type, `Load` throws a
`KeyNotFoundException` listing the bundle's contents.

| Member | Returns |
| --- | --- |
| `Load<T>(name)` | A single asset. |
| `LoadAll<T>()` | All assets of type `T` in the bundle. |
| `LoadWithSubAssets<T>(name)` | The sub-assets of an asset, such as the sprites of a texture or the meshes of a model. |
| `AssetNames` | The full path of every asset in the bundle. |
| `Contains(name)` | Whether the bundle contains an asset with the given name or path. |

A loaded prefab is a template. Use `Object.Instantiate` to create an instance in the scene:

```csharp
var panel = UnityEngine.Object.Instantiate(bundle.Load<GameObject>("Panel"));
```

## Bundle Lifetime

Load each bundle once (typically in `OnGameReady`) and keep the `ModAssetBundle` in a field. Unity loads a bundle file
only once: calling `LoadBundle` with a path that is already loaded returns the same bundle, and two mods cannot load the
same bundle file.

To release a bundle:

```csharp
bundle.Unload();                   // loaded assets remain available
bundle.Unload(unloadAssets: true); // loaded assets are also destroyed
```

Most mods keep their bundles loaded for the entire session.

## Troubleshooting

- **"Asset bundles can be loaded only after the game is ready"**: the bundle was loaded in `OnLoad`, before Unity is initialized. Load
  it in `OnGameReady` or later.
- **"Asset bundles can be loaded only on Unity's main thread"**: the bundle was loaded from another thread or a `Task`. Use a
  [coroutine](./coroutines) or an event handler.
- **"Unity could not load the asset bundle"**: Unity's log (`Player.log`, in
  `%USERPROFILE%\AppData\LocalLow\<developer>\<game>`) states the cause. Typically the bundle was built with a newer
  Unity version than the game's, or another mod has already loaded the same file.

::: details Implementation on IL2CPP games
IL2CPP builds only include the code a game uses, and many games strip Unity's `AssetBundle` class, or most of its
methods, because they do not load bundles themselves. Lustral calls the engine's native functions directly, which are
present in every Unity player, so `Assets` works regardless of whether the game includes the class.
:::
