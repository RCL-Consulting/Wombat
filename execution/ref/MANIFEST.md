# Reference corpus — [PROJECT_NAME]

Everything an agent may retrieve from while working in this project. The harness never
reads what is in here; `lint` only insists that it is catalogued, because material an
agent retrieves from becomes part of the provenance of what it writes, and an entry
nobody recorded is provenance nobody can reconstruct later.

One row per top-level entry of `ref/`. Item, source and licence are all required; a row
missing any of the three fails the lint. Delete a row when its entry goes.

| item | source | licence | added | notes |
|---|---|---|---|---|
| _dir-or-file_ | _where it came from: URL, repo, ISBN, "supplied by client"_ | _SPDX id, or the terms_ | _YYYY-MM-DD_ | _what it is used for_ |
