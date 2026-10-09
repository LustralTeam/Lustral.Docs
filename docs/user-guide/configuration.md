# Configuration

Lustral and mods store their settings in [TOML](https://toml.io) files inside the game's `Lustral` folder. Each file
contains `key = value` entries grouped under `[section]` headers, with comments describing each setting. The files can
be edited with any text editor.

Settings are read when the game starts. Edit them while the game is closed.

## Loader Settings

Lustral creates `Lustral/loader.toml` on the first launch, with every setting and its description:

```toml
[log]
level = "info"        # log detail: "debug", "info", "warn" or "error"
console = false       # true: show the log in a console window while the game runs

[mods]
disabled = []         # IDs of mods not to load
force = []            # IDs of mods to load despite failed requirements

[updates]
mode = "auto"         # "auto", "notify" or "off"
prereleases = false
mods = {}             # per-mod update modes
```

All settings are documented in the [Loader Configuration reference](/reference/loader-config). The most commonly used
are:

- **`console`**: shows the log in a separate console window as it is written.
- **`disabled`**: [disables](./managing-mods#disabling-a-mod) mods without uninstalling them.
- **`[updates]`**: controls [automatic updates](./automatic-updates).

If the file contains an error, Lustral reports it in the log, uses the default value for the affected setting and
starts normally. Deleting the file restores the defaults on the next launch.

### Forcing a Mod to Load

Lustral skips mods whose declared requirements are not met, for example a mod built for another game version, a newer
Lustral version, or one that is incompatible with another installed mod. To load such a mod anyway, add its ID to
`force`:

```toml
[mods]
force = ["someone.fastforward"]
```

The mod is then loaded with a warning in the log. Forced mods may not work correctly or may cause the game to
malfunction; remove the ID from `force` if problems occur.

## Mod Settings

Mods with settings store them in `Lustral/config/<mod id>.toml`. The file is created the first time the mod runs and
lists each setting with its default value and description:

```toml
# Settings for BetterHud.
# Changes take effect the next time the game starts. New settings are added when the mod is updated.

# Show the frame rate in the corner.
show_fps = false

# How loud the mod's sounds are, from 0 to 1.
volume = 0.8

# One of: Low, Medium, High
quality = "Medium"
```

Changes take effect on the next launch. Removing a setting, or the whole file, restores the mod's defaults. Settings
files are preserved when a mod is updated; new settings are added with their default values.
