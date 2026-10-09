# Library Mods

A library mod provides code for other mods, such as a UI toolkit, an API for adding content to a game, or helpers shared
by several mods. Users install it like any other mod. Lustral loads it once, before the mods that depend on it, and all
dependents share the same instance.

```xml
<PropertyGroup>
  <ModId>someone.uikit</ModId>
  <ModKind>library</ModKind>
  <Version>1.2.0</Version>
</PropertyGroup>
```

A library does not require a [`Mod`](/api/mod) class. If it has one, Lustral starts it like any other mod, which is useful
for libraries that patch the game or subscribe to events themselves.

## Exported Assemblies

On IL2CPP games, each mod runs in its own load context, so a mod cannot access another mod's assemblies unless they are
exported. A library exports its own assembly by default. If it ships additional assemblies that dependents should use,
list all of them:

```xml
<ItemGroup>
  <ModExport Include="UiKit" />
  <ModExport Include="UiKit.Themes" />
</ItemGroup>
```

On Mono games, all mods share the game's runtime, so exports do not affect visibility; declare them regardless so that
the library behaves consistently on both backends.

## Referencing a Library

A mod that uses a library declares a dependency on it. With the library installed in the game, no further configuration
is needed: the SDK references the library's exported assemblies from its folder in the game and does not copy them into
the dependent mod.

```xml
<ItemGroup>
  <ModDependency Include="someone.uikit" Version="^1.2" />
</ItemGroup>
```

If the library is a project in the same solution, add a project reference as well; the SDK then uses it instead of the
installed copy:

```xml
<ProjectReference Include="..\UiKit\UiKit.csproj" Private="false" />
```

Static state in the library is shared across all dependents, which allows patterns such as a shared registry of items or
UI panels.

## Versioning

Dependents specify the versions they accept, typically as `^` ranges, so libraries should follow
[semantic versioning](https://semver.org): breaking changes increment the major version (or, before 1.0, the minor
version). A dependent built for `^1.2` is then never loaded against an incompatible 2.0; Lustral skips it and logs the
reason.
