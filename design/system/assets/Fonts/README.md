# Fonts

The licences of Wombat's two faces, copied byte for byte from `src/Wombat.Web/wwwroot/fonts/`, where each sits beside its font files: `SourceSans3-OFL.txt` and `Fraunces-OFL.txt` are in this folder, and the integrator uploads them to the Fonts group (its `files` is empty until then). The WOFF2 files themselves are the system's `fonts/` (tokens.json `type.fonts`), not assets: `design/system/fonts/` holds the three, byte for byte `wwwroot/fonts/`'s, with the SHA-256s `Design/TypographyTests` pins, and they are published with the system at `fonts/`.

- `SourceSans3-OFL.txt`: Source Sans 3, the body face. Adobe's `LICENSE.md` at release 3.052R, whole: "Copyright 2010-2022 Adobe (http://www.adobe.com/), with Reserved Font Name 'Source'", under the SIL Open Font License 1.1. Its files are the release's variable upright and italic WOFF2 (`SourceSans3VF-Upright.ttf.woff2`, `SourceSans3VF-Italic.ttf.woff2`, weights 200 to 900), byte for byte.
- `Fraunces-OFL.txt`: Fraunces, the wordmark's face. The Fraunces project's `OFL.txt`: "Copyright 2018 The Fraunces Project Authors (https://github.com/undercasetype/Fraunces)", under the SIL Open Font License 1.1. Its file is `fraunces-var.woff2` (weights 100 to 900).

## Rules (W-011; DESIGN.md § Files that own the design system)

- Both faces are self-hosted and served as files beside the application, never compiled into it: the CSP's `font-src` is `'self'`, so no font host, and a separately served file is aggregation beside the AGPL-3.0 code.
- **"Source" is a Reserved Font Name.** A subset, converted or otherwise changed Source Sans 3 file is a Modified Version and may not be served under that name: never trim or convert these files; vendor a newer release whole. `Design/TypographyTests` pins each file's SHA-256 to the release's.
- Fraunces reserves no name.
- A new face comes with its licence file beside it; `Design/TypographyTests` fails on a `.woff2` with no licence.
