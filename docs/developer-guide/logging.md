# Logging

Each mod has a logger, [`Log`](/api/i-log), whose entries are tagged with the mod's ID:

```csharp
Log.Debug("Only written when the log level is debug");
Log.Info("Loaded 12 new cosmetics");
Log.Warn("The game version is newer than this mod supports; some features are disabled");
Log.Error("Could not read the save file", exception);
```

```txt
[14:02:11.508] [Info ] [you.betterhud] Loaded 12 new cosmetics
[14:02:11.509] [Warn ] [you.betterhud] The game version is newer than this mod supports; some features are disabled
```

`Error` accepts an exception as its second argument and writes its type, message and stack trace, including line
numbers, since the mod's debug symbols are deployed with it.

## Log Output

- **`Lustral/Logs/latest.log`** in the game folder contains the current session. Logs from the ten previous sessions are
  kept in the same folder, named by their start time (for example `2026-10-08_14-02-09.log`).
- **The console window**, enabled with `console = true` in the `[log]` section of `Lustral/loader.toml`, shows the log
  as it is written, with each mod's name in its own color. It closes with the game.

The log level in `loader.toml` (`[log] level`) determines which entries are written: `debug`, `info` (default), `warn`
or `error`. `Log.Debug` entries are only written at the `debug` level.

## Formatting

Messages can contain Unity rich text tags. The console window renders them; log files contain only the plain text.

```csharp
Log.Info("Health: <color=red>low</color>, shield <b>broken</b>");
Log.Info("<color=#ff8800>Orange</color>, <i>italic</i>, <u>underlined</u>, <s>strikethrough</s>");
```

Colors are Unity color names (`red`, `orange`, `lightblue`, ...) or `#rgb`/`#rrggbb` values, optionally with an alpha
component. As in Unity, a tag is only interpreted when it has a matching closing tag, so text such as `List<int>` is
written unchanged.

The mod's name in the console is assigned a color derived from its ID. To set the color explicitly, use `ModColor` in
the project file, with a Unity color name or `#rrggbb` value:

```xml
<ModColor>orange</ModColor>
```

## Repeated Entries

Entries written repeatedly, such as an error in a per-frame handler, are collapsed: Lustral writes the entry once and
then periodically reports the number of repetitions:

```txt
[14:02:12.001] [Error] [you.betterhud] Update handler threw
[14:02:22.003] [Error] [you.betterhud] (repeated 599 more times)
```

## Recommendations

- Log the information a user needs to report a problem: what the mod did and its outcome, rather than every step.
- Use `Log.Debug` for diagnostic detail, and ask users to set `level = "debug"` when investigating an issue.
- Pass exceptions to `Log.Error` rather than logging `exception.Message`, so that the stack trace is preserved.
