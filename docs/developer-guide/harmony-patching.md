# Harmony Patching

Mods modify game behavior by patching the game's methods. Lustral includes [HarmonyX](https://github.com/BepInEx/HarmonyX),
a fork of [Harmony](https://harmony.pardeike.net), on both scripting backends. Mod projects reference it automatically;
do not add a separate package reference or ship its assembly.

## The Mod's Harmony Instance

[`Mod.Harmony`](/api/mod#harmony) is a Harmony instance identified by the mod's ID. Patches applied through it are
associated with the mod:

- If the mod fails to load (its constructor or `OnLoad` throws), Lustral removes its patches.
- Crash reports list which mod patched which method.

Apply all patch classes in the assembly from `OnLoad`, so that the patches are in place before the game's code runs:

```csharp
public override void OnLoad() => Harmony.PatchAll(typeof(ModEntry).Assembly);
```

Lustral does not apply patches automatically; nothing is patched until the mod calls `PatchAll` or `Patch`.

## Patch Classes

A patch class specifies the target method and contains the patch methods:

```csharp
[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.TakeDamage))]
internal static class TakeDamagePatch
{
    // Runs before the original method. Returning false skips the original and any remaining prefixes.
    private static bool Prefix(PlayerController __instance, ref int damage)
    {
        if (ModEntry.Instance.Settings.HalfDamage)
            damage /= 2;
        return true;
    }

    // Runs after the original method, even if a prefix skipped it.
    private static void Postfix(PlayerController __instance, int __result)
    {
        ModEntry.Instance.Log.Debug($"{__instance.name} took damage, {__result} health left");
    }
}
```

Patch method parameters are matched by name:

| Parameter | Value |
| --- | --- |
| A parameter of the original method, by name (`damage`) | Its value; declare it `ref` to modify it |
| `__0`, `__1`, ... | The original method's parameters by position |
| `__instance` | The instance the method was called on |
| `__result` | The return value (declare it `ref` in a postfix to modify it) |
| `___fieldName` (three underscores) | A field of the instance; declare it `ref` to modify it |

Harmony's documentation covers further topics: [transpilers](https://harmony.pardeike.net/articles/patching-transpiler.html),
finalizers, `TargetMethod` for targets that `[HarmonyPatch]` cannot express, and manual patching with `Harmony.Patch`.

## Overloaded Methods

For overloaded methods, specify the parameter types of the target overload:

```csharp
[HarmonyPatch(typeof(Inventory), nameof(Inventory.Add), typeof(Item), typeof(int))]
```

## Compile-Time Validation

Lustral's analyzers validate `[HarmonyPatch]` classes against the game's assemblies during the build, using the same
resolution rules as Harmony at runtime:

- [LUS0101](/reference/analyzers#lus0101): the target method does not exist, for example because of a typo or a game
  update. Private methods are included.
- [LUS0102](/reference/analyzers#lus0102): the target method is overloaded and the patch does not specify an overload.
- [LUS0103](/reference/analyzers#lus0103): a patch parameter matches neither a parameter of the target method nor one
  of Harmony's special parameters (`__instance` on a static method, `__result` on a `void` method, or `___field` for a
  field that does not exist).

## Private Members

`nameof` can only reference accessible members. Specify private methods by name, and use Harmony's
[`AccessTools`](https://harmony.pardeike.net/articles/utilities.html) or `Traverse` to access private fields and
methods from mod code:

```csharp
[HarmonyPatch(typeof(PlayerController), "RecalculateSpeed")]
```

On IL2CPP games, the interop assemblies expose private members as public, so `nameof` can be used.

## IL2CPP Considerations

Patching works the same way on IL2CPP games, with the following differences resulting from the game's native code:

- **Patch parameters use interop types**: `Il2CppSystem.String` instead of `string`,
  `Il2CppSystem.Collections.Generic.List<T>` instead of `List<T>`. Strings convert implicitly in both directions. See
  [Mono and IL2CPP](./scripting-backends).
- **Inlined methods**: the IL2CPP compiler inlines small methods. A patch on an inlined method only runs when it is
  called from code where it was not inlined. If a patch on a short method, such as a property getter, never runs, patch
  its callers instead.
- **Transpilers are not supported**, since the game contains native code rather than IL. Use prefixes and postfixes.

## Patches That Do Not Run

A patch runs only when the game calls the patched method. If a patch never runs:

- **The method has not been called yet.** Some games wait on their first screen for a key press before they load the
  main menu, so a patch on a menu class runs only after that input. Patch a class from the first scene instead, or
  confirm the patch with the input given.
- **The method is inlined** (IL2CPP games only; see above). Patch its callers instead.
- **The patch targets the wrong overload.** Specify the parameter types (see [Overloaded Methods](#overloaded-methods)).

If a patched method no longer exists, for example after a game update, the mod fails to load and the log names the patch
class and the missing method:

```txt
[Error] [Core] you.betterhud failed to load: DisplayTipPatch patches HUDManager.DisplayTip, which does not exist in this version of the game
```

## Removing Patches

Patches remain active for the entire session. To remove all patches applied by the mod, call `Harmony.UnpatchSelf()`.
