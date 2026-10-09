# Publishing and Updates

## Creating a Release Package

Enable `ModPackage` and build in the Release configuration:

```xml
<ModPackage>true</ModPackage>
```

```sh
dotnet build -c Release
```

The build writes `bin/Release/<framework>/<id>-<version>.zip` and reports its SHA-256 hash:

```txt
you.betterhud package -> C:\...\bin\Release\net10.0\you.betterhud-1.2.0.zip (SHA-256 4f1c...)
```

The archive contains a single folder, named after the mod's ID, with the same contents as the mod's folder in the game:
its assemblies, `mod.toml`, debug symbols and [shipped files](./files-and-data). Users install it by extracting it into
the game's `Mods` folder.

Mods [built for both backends](./multi-backend-mods) produce one package per backend, `<id>-<version>-mono.zip` and
`<id>-<version>-il2cpp.zip`, in `bin/Release/`.

## Automatic Updates

To let Lustral keep the mod up to date, declare its update source:

```xml
<ModUpdateSource>github:you/betterhud</ModUpdateSource>
```

Publishing an update then consists of creating a GitHub release:

1. Increment `<Version>` and build the release package.
2. Create a GitHub release tagged with the version, for example `v1.3.0` or `1.3.0`.
3. Attach the package.

On the user's next launch, Lustral detects the new release, downloads it in the background and installs it on the
following launch, before any mod is loaded.

### Update Validation

Lustral only installs versions that are compatible:

- The download is verified against the SHA-256 hash GitHub records for the file, so corrupted or altered downloads are
  rejected.
- The new version's [requirements](./compatibility) must be met by the user's game, Unity version and Lustral version.
- Its dependencies must be installed at accepted versions, and the update must not leave any installed dependent mod
  with an unsupported version.

Versions that fail validation are skipped, the reason is logged, and Lustral waits for the next version.

### Rollback

If the new version fails to load, or the game crashes during startup in the session that installed it, Lustral restores
the previous version on the next launch and does not offer the failed version again.

### User Preferences

Updates are enabled by default (`auto`). Users can change this for all mods or for individual mods in
`Lustral/loader.toml`:

```toml
[updates]
mode = "auto"              # "notify" only reports new versions in the log; "off" disables checks
prereleases = false        # include GitHub pre-releases
mods = { "you.betterhud" = "notify" }
```

Lustral checks each update source at most once every six hours, and only over HTTPS.

## Selecting the Release Asset

A release can contain multiple files. By default, Lustral selects the release's only `.zip` file, or the `.zip` file
whose name contains the mod's ID. Files named for a scripting backend (`-mono` or `-il2cpp` as a separate word in the
name) are only selected for games using that backend, so multi-backend mods can attach both packages to one release. To
specify the file explicitly, set `ModUpdateAsset`, using `*` as a wildcard:

```xml
<ModUpdateAsset>betterhud-*.zip</ModUpdateAsset>
```

## Update Feeds

Mods not hosted on GitHub can point `ModUpdateSource` to a JSON feed served over HTTPS:

```xml
<ModUpdateSource>https://example.com/betterhud/releases.json</ModUpdateSource>
```

```json
{
  "releases": [
    {
      "version": "1.3.0",
      "url": "betterhud-1.3.0.zip",
      "sha256": "4f1c...",
      "size": 48213,
      "prerelease": false
    }
  ]
}
```

`url` may be relative to the feed's address. `sha256` and `size` are optional, but providing the SHA-256 hash reported
by the build allows Lustral to verify the download. Multi-backend mods list each version twice, with
`"backend": "mono"` and `"backend": "il2cpp"`.

## Licensing

Mods can be distributed under any license. The project template and the sample mods are licensed under
[MIT No Attribution](https://github.com/LustralTeam/Lustral/blob/master/samples/LICENSE), which permits any use without
attribution. Lustral itself is licensed under the
[Mozilla Public License 2.0](https://github.com/LustralTeam/Lustral/blob/master/LICENSE), which applies to Lustral's own
files only, not to mods that use its API. The code examples in this documentation are also licensed under MIT No
Attribution and can be used in mods without restriction.

Declare the mod's license with `PackageLicenseExpression` in the project file, as an
[SPDX license expression](https://spdx.org/licenses/). It is shown with the mod's details:

```xml
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

Mods are subject to the license terms of the game they modify. Do not include the game's assemblies or assets in a
mod's package.

## Versioning

Use [semantic versioning](https://semver.org): `major.minor.patch`, with a suffix such as `1.3.0-beta.1` for
pre-releases. Lustral compares versions accordingly, dependency ranges of other mods rely on it, and updates only ever
move to a higher version.

::: warning Third-party software
Users trust mod authors with each release and update. Lustral verifies that a download matches the published file, not
the behavior of its code. Keep your release process and GitHub account secure.
:::
