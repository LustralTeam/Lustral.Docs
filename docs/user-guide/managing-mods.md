# Managing Mods

Each mod is installed in its own folder inside the game's `Mods` folder, named after the mod's ID. The folder contains
the mod's manifest (`mod.toml`) and its assemblies:

```txt
<game>/
└─ Mods/
   ├─ someone.betterhud/
   │  ├─ mod.toml
   │  └─ BetterHud.dll
   └─ someone.uilib/
      ├─ mod.toml
      └─ UiLib.dll
```

## Installing a Mod

Mods are typically distributed as a ZIP archive containing the mod's folder, for example
`someone.betterhud-1.2.0.zip`. Extract the archive into the `Mods` folder and launch the game.

Mods that support both scripting backends may be distributed as two archives, ending in `-mono` and `-il2cpp`.
Download the one matching the game's backend, as described in the
[Installation Guide](./installation#step-2-download-the-correct-package).

The log lists each loaded mod and a summary:

```txt
[Info ] [Core] Loaded Better HUD 1.2.0 (someone.betterhud)
[Info ] [Core] Loaded 2 of 2 mods
```

Lustral reports common installation mistakes in the log, such as an archive that was not extracted, a mod folder nested
one level too deep (`Mods/someone.betterhud/someone.betterhud/mod.toml`), or a DLL placed directly in `Mods`.

## Dependencies

Some mods depend on other mods, such as a shared library. The mod's documentation lists its dependencies. If a
dependency is missing, the log names it and the required versions:

```txt
[Warn ] [Core] Not loading someone.betterhud: requires someone.uilib ^2.1.0, which is not installed
```

Install dependencies the same way, each in its own folder. Lustral determines the load order automatically.

## Mods That Fail to Load

Before loading a mod, Lustral checks that it supports the game, the game version, the scripting backend and the
installed Lustral version, and that its dependencies are present. If a check fails, the mod is skipped and the log
records the reason:

```txt
[Warn ] [Core] Not loading someone.fastforward: is for BloonsTD6 55.*; the installed version is 56.3
```

Other mods continue to load. The usual resolution is a compatible version of the mod, or an update of the game or of
Lustral. To load the mod regardless, see [Forcing a Mod to Load](./configuration#forcing-a-mod-to-load).

## Disabling a Mod

To keep a mod installed without loading it, add its ID (the name of its folder) to `disabled` in
`Lustral/loader.toml`:

```toml
[mods]
disabled = ["someone.betterhud"]
```

Mods that depend on a disabled mod are also skipped. Remove the ID from the list to enable the mod again.

## Removing a Mod

Close the game and delete the mod's folder from `Mods`. The mod's settings (`Lustral/config/<id>.toml`) and data
(`Lustral/data/<id>/`) are kept for a possible reinstallation; delete them to remove the mod completely.

## MelonLoader and BepInEx Mods

Mods built for other mod loaders are not compatible with Lustral. Lustral mods are identified by their `mod.toml`
manifest.
