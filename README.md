# Lustral Documentation

Source of the documentation website for [Lustral](https://github.com/LustralTeam/Lustral), built with
[VitePress](https://vitepress.dev) and deployed to GitHub Pages by `.github/workflows/deploy.yml` on every push to
`master`. GitHub Pages must be configured with **Settings > Pages > Source: GitHub Actions**.

## Development

```sh
npm install
npm run dev      # local preview with live reload
npm run build    # static site in docs/.vitepress/dist
npm run api      # regenerate the API reference
```

`npm run api` generates `docs/api` from Lustral.Api's XML documentation, using a Lustral checkout next to this
repository (`../Lustral`). Run it whenever the public API changes and commit the result.

## Structure

| Path | Contents |
| --- | --- |
| `docs/user-guide` | Installation and usage documentation for users |
| `docs/developer-guide` | Guides for mod developers |
| `docs/reference` | SDK properties, file formats, version ranges and analyzers |
| `docs/api` | API reference (generated) |

## Contributing

Questions about the documentation can be asked on the [Lustral Discord server](https://discord.gg/3Y6Ete73E3). Submit changes as pull
requests. Before submitting, check that `npm run build` succeeds. Pages are written in a formal,
neutral style, with headings in title case. Code examples must compile against the current API, and quoted log or error
messages must match the messages in the current Lustral version. Contributions to Lustral itself are described in its
[CONTRIBUTING.md](https://github.com/LustralTeam/Lustral/blob/master/CONTRIBUTING.md).

## License

The documentation is licensed under [MIT No Attribution](LICENSE), so that its text and code examples can be used
without restriction, including in mods. Lustral itself is licensed under the
[Mozilla Public License 2.0](https://github.com/LustralTeam/Lustral/blob/master/LICENSE).
