# Analyzers

Lustral's Roslyn analyzers are included with the SDK and validate mods at design time and during the build, detecting
errors that would otherwise only occur at runtime. Each diagnostic has an ID. To suppress a diagnostic for a project,
add `<NoWarn>$(NoWarn);LUS0101</NoWarn>`; for a single location, use `#pragma warning disable LUS0101`.

## Mod Class

### LUS0001 {#lus0001}

**A mod requires a class deriving from `Lustral.Mod`** (error). Lustral starts a mod through its `Mod` class, and none was
found. Add one, or set `<ModKind>library</ModKind>` for a [library mod](/developer-guide/library-mods).

### LUS0002 {#lus0002}

**A mod must have exactly one `Lustral.Mod` class** (error). Lustral creates exactly one mod class per mod, and multiple were found.
Keep one and call the other classes from it.

### LUS0003 {#lus0003}

**`Lustral.Mod` classes require a public parameterless constructor** (error). Lustral instantiates the mod class through
its parameterless constructor.

## Harmony Patches

These diagnostics validate `[HarmonyPatch]` classes using the same resolution rules as Harmony at runtime. Classes that
determine their target in code (`TargetMethod`, `TargetMethods`) or specify the type by name are not validated.

### LUS0101 {#lus0101}

**Harmony patch target not found** (warning). The specified method does not exist, including in base types, which
typically indicates a typing error or a member renamed in a game update. Reported only where all members are visible:
IL2CPP interop assemblies and the mod's own code. Private members of Mono game assemblies are not visible to the
compiler and are therefore not reported.

### LUS0102 {#lus0102}

**Harmony patch target is ambiguous** (warning). The target method is overloaded and the patch does not specify an
overload. Add the parameter types:

```csharp
[HarmonyPatch(typeof(Inventory), nameof(Inventory.Add), typeof(Item), typeof(int))]
```

### LUS0103 {#lus0103}

**Harmony patch parameter does not match the target** (warning). A parameter of a prefix, postfix or finalizer cannot
be supplied by Harmony: it matches neither a parameter of the target method (by name, or `__0`, `__1`, ... by position)
nor one of Harmony's special parameters (`__instance`, `__result`, `__state`, `___field`, ...), or it is not applicable
to the target (`__instance` on a static method, `__result` on a `void` method, or `___field` for a field that does not
exist).

## Settings

### LUS0201 {#lus0201}

**Setting type cannot be stored in a settings file** (warning). A property of a class passed to `Config.Bind` or
`Config.Save` has an unsupported type. Use `bool`, `string`, a numeric type, an enumeration, an array or list of these
types, or a nested settings class. See [Configuration](/developer-guide/configuration).

## IL2CPP

### LUS0301 {#lus0301}

**Unity does not call message methods without `[Il2CppMethod]`** (warning). In an `[InjectedType]` MonoBehaviour, Unity
only calls methods marked `[Il2CppMethod]`, so an `Update` or `OnTriggerEnter` method without the attribute is never
called. See [Custom Components](/developer-guide/custom-components#il2cpp).

### LUS0302 {#lus0302}

**An `[Il2CppField]` requires an IL2CPP type** (error). `[Il2CppField]` properties are stored on the IL2CPP object, so their
type must be an IL2CPP type: `Il2CppSystem.Int32` instead of `int`, `Il2CppSystem.String` instead of `string`, or a Unity
or game type. The message names the type to use. For .NET objects, such as a `List<string>` or a custom class, use
`[ManagedField]` instead.
