# MsfCoverage

MSF coverage, the programme's (`/msf/coverage`, `Pages/MultiSourceFeedback/ProgrammeCoverage.razor`; its words in `MsfProgrammeCoverageText.cs`): **per programme, EPA and semester, how many of the programme's trainees a released MSF campaign covered**, for a coordinator planning the next campaigns. Coverage, never a shortfall. Flow 10 (T369, 2026-10-19, 68327c9d; boards R3-V1 to V5; round 3 S13; T210, T329; build review D10) rewrote it. Until then it had a "Back to campaigns" button, a covered cell was told apart by nothing, the two grids were unnamed regions, and a failed read printed the exception's text. (This is not the committee review's MSF coverage panel, `MsfCoveragePanel`, which is still not recorded.)

## Who reads it

Coordinator and Administrator, as the campaign pages; reached from the campaign list's header (MsfCampaigns), not the menu; owned by MSF campaigns: Home › MSF campaigns › MSF coverage. The trainees are those the caller may read about who are on the programme now (T238, T268): no graduate, no withdrawn trainee, no locked or erased account. Someone who holds Trainee reads no other trainee's record: the page is a standing warning (`#msf-coverage-trainee`, `Role=""`), "You hold the Trainee role, so you see no other trainee's multi-source feedback coverage, whatever other role you hold. Your own is on My progress.", and asks for nothing.

## The page

- **The header**: "MSF coverage", the subtitle "How many of each programme's trainees a released multi-source feedback campaign covered, per EPA and semester. Coverage is not a target."; no action ("Back to campaigns" went: the trail is the way back).
- **The opening** (`p.muted#msf-coverage-intro`, one C# string): what covered means, that each trainee is counted from their own progress page, that MSF counts towards no target so an uncovered trainee is no shortfall, when a trainee counts in a semester, and that an ended semester can still gain a campaign released later. Kept at six sentences by round 1's decision (build review D10).
- **One card a programme** (`section.detail-card.coverage-programme` in `.msf-page`, named by its `h2`, "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital": a curriculum as one institution follows it, so an Administrator sees one a institution):
  - `ul.coverage-windows` (muted): each semester's trainees, the semester's name at 600 in `text-color`: "**Semester 1, 2026:** 5 trainees, whose programme had started by 30 June 2026"; "No trainee on the programme had started by …".
  - **By EPA** (`h3`) and **By trainee** (`h3`), each a `.clinic-table.clinic-table--compact.coverage-table` in a `.table-container` that is a **named, focusable scroll region** (`tabindex="0"`, `role="region"`, `aria-labelledby` its `h3` and then the programme's `h2`, so each region says whose it is): a row header per row (`th scope="row"`, at least 16rem), a column per semester (today's and the one before), each cell's figure on one line.
    - By EPA: the code bold over its title (`muted text-sm`; an institution's own EPA "The institution's own EPA" under it), then "1 of 5 trainees covered"; "No trainee had started" where none counts, never "0 of 0". Captioned "Each EPA of the programme, by semester: of the trainees who count in the semester, how many had a released MSF campaign covering the EPA close in it."
    - By trainee: the name, then "2 of 15 EPAs covered" (the count their own My progress "This period" line gives), or "Not started". Captioned "Each trainee on the programme, by semester: how many of its EPAs a released MSF campaign covered for them, the count their own progress page gives."
  - **A covered cell is bold by its class alone** (`td.is-covered`, where the count is above 0; V1). Like the trainee's grid it takes no badge, no warning tint and no bar: nothing here is a verdict against a target.
  - A programme with no EPA in use: "No EPA on this programme is in use at the moment, so there is no coverage to count."

## States (T329)

- **Loading**: an always-present status, "Loading MSF coverage.", and five skeletons.
- **Load failed**: "Could not load MSF coverage. Nothing has changed. Try again, or come back in a few minutes." with Try again, never the exception's text, which goes to the log.
- **None**: the empty card, "No trainees to show", "No trainee whose record you may read is on a programme now. A trainee who has completed or left a programme is not counted.".

## Narrow widths

**The one exception to "nothing scrolls sideways" at 390** (S13): a grid scrolls inside its named, focusable region, never the page. Below 641px a row header narrows to 12rem (`.msf-page .coverage-table th[scope="row"]`), and the regions still scroll.

## Contrast

No new colour token. Cells 12.63:1 on the surface, bold where covered (weight, not colour); row headers and the EPA titles as DataTable's; the opening and the captions `muted-text` 4.83:1 on the page, 5.09:1 on the surface.

## Known gaps

- **Kept:** each semester's line writes its last day long, "whose programme had started by 30 June 2026" (`QuotaText.LongDate`): D1's ISO date is for the screens a registrar reads, and the coordinator's coverage text keeps its long dates (`QuotaText.Iso`'s note).
- The opening and the captions say "their" of a trainee, a pronoun the voice avoids.
