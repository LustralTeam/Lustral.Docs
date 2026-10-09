# Automatic Updates

Mods can be updated automatically. A mod that declares an update source in its manifest (usually its GitHub
repository) is checked for new versions by Lustral. Mods without an update source must be updated manually.

## How Updates Work

By default, for each mod with an update source:

1. When the game starts, Lustral checks for a newer version, at most once every six hours.
2. If one is available, it is downloaded in the background and verified against the file the author published.
3. On the next launch, Lustral installs the new version before any mod is loaded.

The log records each step:

```txt
[Info ] [Updates] Downloaded Better HUD 1.3.0 (someone.betterhud); it will be installed the next time the game starts
```

Lustral only installs versions that are compatible with the game, the game version and the installed Lustral version,
whose dependencies are installed at accepted versions, and that do not break other installed mods depending on them.
Incompatible versions are skipped, and the log records the reason.

### Failed Updates

If a new version fails to load, or the game crashes during startup right after an update, Lustral restores the
previous version on the next launch and does not install the failed version again:

```txt
[Warn ] [Updates] Better HUD 1.3.0 failed to load; Lustral restores 1.2.0 the next time the game starts and will not install 1.3.0 again
```

The previous version of each updated mod is kept in `Lustral/updates/previous/` for this purpose.

## Update Settings

Updates are configured in `Lustral/loader.toml`:

```toml
[updates]
mode = "auto"
prereleases = false
mods = {}
```

| Setting | Description |
| --- | --- |
| `mode` | `"auto"` downloads and installs new versions; `"notify"` only reports them in the log; `"off"` disables update checks. |
| `prereleases` | `true` also installs versions the author marked as pre-releases. |
| `mods` | Overrides `mode` for individual mods, by ID. |

Example:

```toml
mods = { "someone.betterhud" = "notify", "someone.uilib" = "off" }
```

::: warning Updates install new code
An update installs a new version of a third-party program without asking for confirmation. Lustral verifies that the
download matches the author's published file, not what the code does. Use `"notify"` to review each version before
installing it.
:::

## Manual Updates

Close the game, delete the mod's folder from `Mods` and install the new version as described in
[Managing Mods](./managing-mods#installing-a-mod). The mod's settings in `Lustral/config` are preserved.
