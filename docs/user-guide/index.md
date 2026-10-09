# User Guide

Lustral is a mod loader for Unity games. Once installed in a game, it loads the mods placed in the game's `Mods`
folder each time the game starts. This guide covers installing Lustral, managing mods and resolving common issues.
For information on creating mods, see the [Developer Guide](/developer-guide/).

## Compatibility

Lustral supports Unity games on Windows (x64). Linux support is planned.

Unity games use one of two scripting backends, both of which Lustral supports. The backend determines which Lustral
package to install (see the [Installation Guide](./installation)):

- **Mono**: Unity 2019.4 and later.
- **IL2CPP**: Unity 2022.3 and Unity 6. Earlier versions have not been tested.

Mods are usually built for a specific game. Lustral checks each mod's declared requirements and does not load mods
built for a different game or game version; the log records the reason.

Lustral only loads mods built for Lustral. MelonLoader and BepInEx mods are not supported.

::: info Pre-release
Lustral has not been publicly released yet. This documentation describes its first release.
:::

## Safety

::: warning Mods are third-party software
Mods run with the same permissions as the game. Lustral does not verify their behavior and is not responsible for any
damage they cause. Only install mods from sources you trust.
:::

**Online games.** Many online games prohibit mods, and some suspend accounts that use them. Games with anti-cheat
software may refuse to start with a mod loader installed. Review the game's terms of service before using mods online.

## Contents

1. [Installation Guide](./installation): installing, updating and uninstalling Lustral.
2. [Managing Mods](./managing-mods): installing, disabling and removing mods.
3. [Configuration](./configuration): Lustral's settings and mod settings.
4. [Automatic Updates](./automatic-updates): keeping mods up to date.
5. [Troubleshooting](./troubleshooting): diagnosing problems and reporting issues.
