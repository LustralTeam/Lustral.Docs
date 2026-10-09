# Mod Dependencies

Mods can depend on other mods. A mod declares which mods it requires, which it uses when present, and which it is
incompatible with. Lustral determines the load order and skips mods that cannot be loaded, recording the reason in the
log.

```xml
<ItemGroup>
  <ModDependency Include="someone.uikit" Version="^1.2" />                 <!-- required -->
  <ModDependency Include="someone.extras" Version="*" Optional="true" />    <!-- used when present -->
  <ModIncompatibility Include="someone.otherhud" />                          <!-- incompatible -->
</ItemGroup>
```

`Version` is a [version range](/reference/versions): `^1.2` accepts 1.2 and later, but not 2.0 (before 1.0, `^0.3`
accepts 0.3.x). `*` accepts any version.

## Required Dependencies

A required dependency must be installed at an accepted version and must load successfully. Otherwise, the dependent mod
is not loaded:

```txt
[Warn ] [Core] Not loading you.betterhud: requires someone.uikit ^1.2, which is not installed
[Warn ] [Core] Not loading you.betterhud: requires someone.uikit ^1.2, but version 1.1.0 is installed
[Error] [Core] Not loading you.betterhud: requires someone.uikit, which failed to load
```

## Optional Dependencies

An optional dependency is not required to be installed. When it is installed at an accepted version, it loads before
the dependent mod, which can check for it and use it:

```csharp
if (Mods.IsLoaded("someone.extras"))
    EnableExtrasSupport();
```

If it is installed at a version outside the accepted range, the dependent mod is not loaded, so that it never runs
against an unsupported version.

## Incompatibilities

```xml
<ModIncompatibility Include="someone.otherhud" />
<ModIncompatibility Include="someone.oldlib" Version="&lt;2.0" />
```

If an incompatible mod is loaded (at a matching version; all versions by default), the declaring mod is not loaded. An
incompatibility only ever affects the mod that declares it, never the other mod.

## Load Order

Lustral loads each mod after its dependencies and any installed optional dependencies. Among mods that do not depend on
each other, `ModLoadOrder` determines the order (lower values first, 0 by default), followed by the mod ID.

```xml
<ModLoadOrder>-10</ModLoadOrder>
```

The load order determines the order of `OnLoad`, `OnGameReady`, per-frame events and Harmony patches applied in
`OnLoad`. Most mods do not need to set it.

## Checking for Other Mods

[`Mods`](/api/loaded-mods) lists the mods loaded so far, in load order, with their [`ModInfo`](/api/mod-info):

```csharp
if (Mods.Find("someone.extras") is { } extras)
    Log.Info($"Using {extras.Name} {extras.Version}");

foreach (var mod in Mods)
    Log.Debug($"{mod.Id} {mod.Version}");
```

In the constructor and `OnLoad`, mods that load later are not yet included. Dependencies and optional dependencies are
always included.

## Using Another Mod's Code

To use another mod's classes, that mod must be a [library mod](./library-mods), which declares the assemblies other mods
may use. With the library installed in the game, declare a dependency on it: the SDK references its exported assemblies
from the installed copy without copying them into the dependent mod (Lustral loads them from the library's folder).

```xml
<ItemGroup>
  <ModDependency Include="someone.uikit" Version="^1.2" />
</ItemGroup>
```

```csharp
using UiKit;
```

Set `Reference="false"` on the `ModDependency` to omit the assembly references, for dependencies that only need to load
first.
