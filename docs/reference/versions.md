# Version Ranges

Mods, Lustral and dependency declarations use [semantic versioning](https://semver.org): `major.minor.patch`, with an
optional pre-release identifier (`1.3.0-beta.1`) and build metadata (`1.3.0+abc123`, ignored in comparisons). `1.2` and
`1` are interpreted as `1.2.0` and `1.0.0`.

Version ranges, used in dependencies, incompatibilities and requirements, follow the syntax used by npm and Cargo:

| Range | Matches | Notes |
| --- | --- | --- |
| `^1.2.3` | `>=1.2.3 <2.0.0` | Compatible with 1.2.3: the same major version. |
| `^0.3.1` | `>=0.3.1 <0.4.0` | Before 1.0, the same minor version, since minor versions may introduce breaking changes. |
| `~1.2.3` | `>=1.2.3 <1.3.0` | The same minor version. |
| `1.2.3` | Same as `^1.2.3` | A version without an operator is treated as `^`, as in Cargo. |
| `=1.2.3` | `1.2.3` only | |
| `>=1.2.0 <2.0.0` | Both conditions | Comparison operators: `>`, `>=`, `<`, `<=`, `=`, separated by spaces or commas. |
| `*` | Any version | |

In project files, `<` must be escaped as `&lt;` inside attributes: `Version="&lt;2.0"`.

## Game and Unity Versions

Game versions (`ModGame` `Versions`) are not semantic versions, as games use arbitrary formats (`v73`, `1.24.0.7`,
`56.3`). They are therefore matched as text, with `*` as a wildcard: `56.*`, `v73*`.

Unity versions (`ModUnityVersion`) use the range syntax above: `>=2022.3`, `^6000.0`.
