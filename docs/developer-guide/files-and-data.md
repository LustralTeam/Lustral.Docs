# Files and Data

Each mod has two folders: its installation folder, for the files it ships, and a data folder, for files it writes at
runtime.

| | Location | Purpose | On update |
| --- | --- | --- | --- |
| [`Info.Directory`](/api/mod-info#directory) | `Mods/<id>/` | The mod's assemblies and shipped files: asset bundles, data tables, translations. | Replaced by the new version. |
| [`Info.DataDirectory`](/api/mod-info#datadirectory) | `Lustral/data/<id>/` | Files the mod writes at runtime: caches, saved data, downloads. | Preserved. |
| [`Config.FilePath`](/api/mod-config#filepath) | `Lustral/config/<id>.toml` | The mod's [settings](./configuration), edited by users. | Preserved. |

## Shipped Files

Every project file copied to the build output is deployed to the mod's folder under the same relative path. To include
all files in an `Assets` folder:

```xml
<ItemGroup>
  <None Include="Assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

The build copies `Assets\names.json` to `Mods/<id>/Assets/names.json` in the game, and [release packages](./publishing)
contain it at the same location. To read it:

```csharp
var path = Path.Combine(Info.Directory, "Assets", "names.json");
var names = File.ReadAllText(path);
```

Asset bundles are loaded with [`Assets.LoadBundle`](./asset-bundles), which accepts a path relative to the mod's folder.

## Runtime Data

Do not write to the mod's installation folder: updates replace it entirely, and any files written there are lost. Use
the data folder instead, which Lustral creates on first access:

```csharp
var history = Path.Combine(Info.DataDirectory, "history.json");
File.WriteAllText(history, json);
```

## Settings

For values users are expected to change, use [settings](./configuration) rather than a custom file. Lustral generates
the settings file with descriptions, adds settings introduced by updates, and reports invalid entries to the user.
