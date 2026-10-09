# Configuration

Mod settings are defined as a C# class. [`Config.Bind`](/api/mod-config#bind) reads them from a TOML file in the game
folder, `Lustral/config/<id>.toml`, which users can edit. If the file is missing or incomplete, Lustral writes it with
every setting and its description.

```csharp
using System.ComponentModel;

public sealed class Settings
{
    [Description("Show the frame rate in the corner.")]
    public bool ShowFps { get; set; }

    [Description("How loud the mod's sounds are, from 0 to 1.")]
    public float Volume { get; set; } = 0.8f;

    public Quality Quality { get; set; } = Quality.Medium;

    public List<string> FavoriteItems { get; set; } = ["flashlight", "shovel"];

    [Description("Where the HUD goes.")]
    public HudSettings Hud { get; set; } = new();
}

public enum Quality { Low, Medium, High }

public sealed class HudSettings
{
    public double Scale { get; set; } = 1;
    public bool Visible { get; set; } = true;
}
```

```csharp
public override void OnLoad()
{
    var settings = Config.Bind<Settings>();
    if (settings.ShowFps)
        Events.OnGUI += DrawFps;
}
```

The generated file:

```toml
# Settings for BetterHud.
# Changes take effect the next time the game starts. New settings are added when the mod is updated.

# Show the frame rate in the corner.
show_fps = false

# How loud the mod's sounds are, from 0 to 1.
volume = 0.8

# One of: Low, Medium, High
quality = "Medium"

favorite_items = ["flashlight", "shovel"]

# Where the HUD goes.
[hud]

scale = 1.0

visible = true
```

## Mapping Rules

- Every public property with a getter and a setter is a setting. Its key in the file is in snake_case: `ShowFps`
  becomes `show_fps`.
- The property's initial value is the default.
- A `[Description]` attribute (from `System.ComponentModel`) is written as a comment above the setting.
- Supported types: `bool`, `string`, integer types (`int`, `long`, ...), floating-point types (`float`, `double`,
  `decimal`), enumerations (written by name, with the allowed values in a comment), arrays and lists of these types,
  and nested classes, which are written as TOML tables (`[hud]`). Unsupported types are reported at build time
  ([LUS0201](/reference/analyzers#lus0201)).
- Settings whose default is `null` (such as an unset `string?` or `int?`) are omitted from the file until a user sets
  them.

## Reading Settings

`Bind` reads the file on every call and returns a new object. Call it in `OnLoad` and keep the result.

Lustral handles invalid input as follows:

- **Missing settings** keep their defaults and are added to the file, so that users see settings introduced by an
  update. Comments added by users are not preserved when the file is rewritten.
- **Invalid values** (`volume = "loud"`, an undefined enumeration name, an out-of-range number) are logged with the
  setting's name and ignored; the setting keeps its default.
- **Unknown settings** (typing errors, or settings removed in a newer version) are logged so that users can correct
  them.
- **Files that are not valid TOML** are logged and left unchanged, and all settings keep their defaults. Lustral never
  overwrites a file it cannot parse.

```txt
[Warn ] [you.betterhud] you.betterhud.toml: volume = "loud" is not a number; using 0.8
[Warn ] [you.betterhud] you.betterhud.toml: unknown setting show_fsp; check the spelling
```

Enumeration names are matched case-insensitively: `quality = "high"` is accepted.

## Saving Settings

Mods with an in-game settings interface can modify the settings object and save it:

```csharp
_settings.Volume = 0.5f;
Config.Save(_settings);
```

`Config.FilePath` returns the file's full path, for display to users.

## File Location

Settings are stored in `Lustral/config`, outside the mod's folder, so they are preserved when the mod is updated or
reinstalled. Each mod has a single settings file; use nested classes to group related settings.

Lustral also writes `<id>.schema.json` next to the settings file. It describes each setting (type, default value,
description and allowed values) so that tools such as mod managers can display a settings interface. No action is
required to generate it.
