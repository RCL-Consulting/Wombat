# RegistrarsCard

Home's Registrars card (`Components/Shared/Programme/RegistrarsCard.razor`): Programme trainees' first five on the Committee member's and both speciality admins' Homes, spanning three, its badge the count of people in words. Flow 06 (T358, 2026-10-05, 85d5a508; Q1; R2-Home c1, c4, c6, c7, c11; round 3 items 27, 31, 33; D10) made it; until then the card was "Targets this period", every trainee by name beside "semester 1/5 · yearly 0/1", no row a link.

## What the consumer provides

`<RegistrarsCard Card="@Summary?.Registrars" IsLoading="IsLoading" />`

- `Card`: the first five and the count (`RegistrarsCardDto`), null until the dashboard's read has come.
- `IsLoading`: the dashboard's read has not returned: the title over the frame's skeleton.

## The card (a DashboardCard)

- **Title** "Registrars" (`users`, `#card-registrars`, spanning three), a section named by it.
- **Badge** in words, "5 registrars" ("1 registrar"), in the draft tone (`BadgeTone="BadgeState.Draft"`, `badge-draft`): a count of people waits on nobody, so it is not `badge-submitted` (D10). None when there is nobody; not while loading.
- **The rule line**, `p.needs-you-rule`: "Fewest met first, then by surname."
- **The rows**: RegistrarRoster, then "n more in Programme trainees." past five.
- **The foot**, `div.dashboard-card-footer`: "Open Programme trainees" (`btn-sm btn-outline`, `/programme/trainees`), **empty included**, since the page is in the menu (item 27). 44px below 641px.
- **Empty**: `p.card-empty` "No current registrars. They appear here once they are admitted to the programme.", then the foot.

## Where it stands (DESIGN.md § Dashboard page, "The oversight Homes")

| Home | Cards, in order |
|---|---|
| Committee member | Registrars, Targets by EPA (each spanning three) |
| Speciality admin, Sub-speciality admin | Waiting for assessors (AssessorsWaitingCard), Registrars, Targets by EPA (each spanning three) |

Each Home is one read behind DashboardFrame, read as that Home's own role in its scope (the Committee member the institution; the admins the speciality's or sub-speciality's registrars there; E4), never the union of the roles held. "Pending reviews", "Trainees in programme", "Curriculum coverage" and every "inactive" are gone (Q3, Q10).

## Rules

- A card that previews a list shows its first five rows, then "n more in <list>.", and its foot opens the list whenever the list is in the menu.
- The counts are badges in words, none when there is nothing (DashboardCard's `BadgeWords`).

## Contrast

The badge's words 11.36:1 on `header-bg`, its edge 3.42:1; the rule line `muted-text` 5.09:1; the foot's outline button 4.86:1.

## Known gaps

- The empty words use a pronoun, "They appear here once they are admitted to the programme." (`ProgrammeWords.RosterEmpty`), against round 3's check 1 (ProgrammeTrainees).
