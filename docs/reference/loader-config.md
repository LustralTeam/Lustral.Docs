# Loader Configuration

`Lustral/loader.toml` in the game folder contains Lustral's settings for that game. Lustral creates it with default
values and descriptions on the first launch and reads it on every launch. The file is managed by the user; mods do not
modify it.

```toml
# Lustral configuration for this game. Changes take effect the next time the game starts.

[log]
# Minimum level written to Lustral/Logs/latest.log: "debug", "info", "warn" or "error".
level = "info"
# Show the log in a console window while the game is running.
console = false

[mods]
# IDs of mods that should not be loaded, for example ["sxxr0.betterhud"]. Mods that depend on them are also skipped.
disabled = []
# IDs of mods to load even if their requirements are not met (a different game, game version, platform or
# Lustral version) or they are incompatible with another installed mod. Forced mods may not work correctly.
force = []

[updates]
# Update behavior for mods that declare an update source:
#   "auto"    download new versions in the background and install them on the next launch;
#             a version that fails to load is rolled back
#   "notify"  report new versions in the log only
#   "off"     do not check for updates
# Mods and their updates are third-party software. Lustral does not verify their behavior and is not
# responsible for any damage they cause. Only install mods from sources you trust.
mode = "auto"
# Also install pre-release versions.
prereleases = false
# Per-mod update modes, for example { "sxxr0.betterhud" = "notify" }.
mods = {}
```

## [log]

| Key | Default | Description |
| --- | --- | --- |
| `level` | `"info"` | The minimum level written: `"debug"`, `"info"`, `"warn"` or `"error"`. |
| `console` | `false` | Shows the log in a console window, with colors, as it is written. |

## [mods]

| Key | Default | Description |
| --- | --- | --- |
| `disabled` | `[]` | IDs of mods not to load. Mods depending on them are not loaded either. |
| `force` | `[]` | IDs of mods to load regardless of their [requirements](/developer-guide/compatibility) and incompatibilities. |

## [updates]

| Key | Default | Description |
| --- | --- | --- |
| `mode` | `"auto"` | For mods with an update source: `"auto"` installs new versions on the next launch, `"notify"` only logs them, `"off"` disables checks. |
| `prereleases` | `false` | Includes pre-release versions. |
| `mods` | `{}` | Overrides `mode` for individual mods, by ID. |

See [Automatic Updates](/user-guide/automatic-updates).

## Invalid Entries

Invalid values are reported in the log at startup, and the affected settings use their defaults. If the file is not
valid TOML, the error is logged and all settings use their defaults. Lustral does not modify the file after creating it.
