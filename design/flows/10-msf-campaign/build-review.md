# Flow 10 build review (T369 step 6)

Three read-only reviewers on Sonnet reviewed the branch `t369` after § 0 (`dae634f0`) and the three page lanes merged (R
`7a8d0e0e`, K `fc45569d`, T `f145d5bb`) and, for G, the runbook lane (`80af774c`): **R** covered the code, the data and
authorization; **D** the pages, the design system and accessibility; **G** the runbook, the cast and regressions.
Severity: H high, M medium, L low; P marks one the reviewer could not fully trace. **No high finding.** Anonymity holds:
no report before release reaches the trainee, no result before close reaches anyone, and a suppressed group shows
nothing on any trainee page. The fix pass (`t369-fix`) does what "The fix pass" below says; the tables keep the
reviewers' own suggestions.

**The operator, 2026-10-15, on R7:** keep the per-group invited and responded counts on an open campaign's page. The
coordinator invited each person by address and chases them with Resend; what they said stays hidden by F1 and the floor.

## R: the code, the data and authorization

**Sound:** `BuildReport` returns no Categories for Draft, Open and a campaign withdrawn while open (ClosedOn null); tests cover Draft, Open, an Administrator reader and the withdrawn cases; `ReadyForRelease` still computed from the full list. The trainee path: `CanReadReportAsync` null for the subject unless Released; an unknown or out-of-scope id null; `MyMsfReport` renders only `IsSubjectsCopy && Released && ReleasedOn`, otherwise redirects with `?unavailable=true`, so an id confirms nothing. `MsfGroupCards` filters suppressed groups; her page and `ListMsfCampaignsForTrainee` carry no identity and no count; Distribution gone. Close and Release call `EnsureCampaignIsInScopeAsync` first, stage nothing before their refusals; the release mail after the save via `AccountNotices` (failures logged), holding no feedback; F4 read before staging. F2's floor for both kinds. F5's migration, Designer and snapshot agree; `ErasureExecutor` pseudonymises `ClosedByUserId`. Auto-close and the link's last day on `ProgrammeCalendar.DateOf`, tested at 22:30 UTC.

| # | Sev | Finding | Fix |
|---|---|---|---|
| R1 | M | `CampaignReport.razor:710-714` `Refusal()` shows `RefusalText.Of(exception)` for every `InvalidOperationException`; `MsfCampaignText.RefusalOf` (`:502`, NewCampaign, CampaignEdit, CampaignsList) prints the message of every `InvalidOperationException`/`UnauthorizedAccessException`. Framework errors (DbContext concurrency, "Sequence contains no elements") and the page's own "The current user identifier is missing." (`CampaignReport.razor:676`) print as refusals, against T329. Flow 09's R1 again | Classify: `ValidationException`, `UnauthorizedAccessException` and the handlers' own refusal sentences show their words; anything else the fixed sentence, logged; the missing id a fixed `ActionFailed` |
| R2 | L | Two fixed failure sentences: `MsfCampaignText.ActionFailed` (`:213`) "Something went wrong. Nothing has changed. …" vs `MsfReportText.ActionFailed` (`:371`) "Something went wrong, and nothing was changed. …" | One, in `MsfCampaignText`; `MsfReportText` uses it; tests follow |
| R3 | L | `CloseMsfCampaign.cs:41` records null when the principal has no `NameIdentifier`: the report then says "Closed by itself" for a close by hand | Refuse (`UnauthorizedAccessException`) before staging, as Release does |
| R4 | L | F2's floor only at create; rows at 1 or 2 (scenario data) still show groups of one or two answers | Nothing is live: the replay regenerates the data; no migration |
| R5 | L (P) | `MsfInvitation.IsReminderDue` (`MsfInvitation.cs:231`) compares the UTC date with the South African last day: a reminder can go out for a link already refused as expired, for two hours after SAST midnight | `ProgrammeCalendar.DateOf(utcNow)`, a 22:30 UTC test |
| R6 | L (P) | The subject's `MsfCampaignAggregateReportDto` still carries `TotalResponses`, suppressed groups' `ResponseCount`, `InvitedTotal`, `SurvivingCategoryCount`, `ClosedByUserId`; only the page keeps them out | When `callerIsSubject`, drop suppressed groups and zero the counts in `GetCampaignAggregateReport`; a handler test |
| R7 | L (P) | `GetMsfCampaignSetup` gives the coordinator per-group invited/responded counts while Open: a group of one invitee shows who answered (T217's count, not F1's results) | Decision for the operator |
| R8 | L | Test gaps: a subject's non-released report is null; an out-of-institution Coordinator gets null from the report; Close with no caller id (R3); `ListMsfCampaignsForTrainee` empty for an out-of-scope reader | Three or four Application tests |

## D: the pages, the design system and accessibility

**Sound:** no raw hex, `style=`, `<i class="bi">`, inline script or `onclick` in the four Flow 10 sections or the nine pages. F3 (no Responses column on her list), F6 (the required mark and `aria-required` on every control F6 names, none on the group select), F7 (the stripe classes), F8 (Close danger, Release primary in their dialogs; outline triggers), F9 ("Open its campaign page" on its own line, 44 px at 390), F11 ("Invitee added." alone), M10 (dialogs inside `.msf-page`, `column-reverse`, 44 px at 390). `aria-disabled` with reasons on Open, Add invitee, Create and Release; the only native `disabled` are K13's. One h1 per state, no skipped levels; tables captioned with row headers and `data-label`s; `msf-group-*` ids unique; ISO dates, "hh:mm SAST"; the narrative counts through `review-appeal.js` without `maxlength`. The Draft and Withdrawn report lines and the over-limit sentence read correctly. Accepted: plain leads in `StatePanel`'s shared load error, the respondent help as `.page-subtitle`, an `Alert` per state.

| # | Sev | Finding | Fix |
|---|---|---|---|
| D1 | M (P) | `.msf-page :is(p, h2, .alert):not(dialog *)` (0,2,1) beats single-class margins: `.list-section-title` dead, so "Campaigns (n)" (`CampaignsList.razor:57`) and "Your released reports" (`MyMsfReports.razor:49`) sit on their tables; `.msf-card-title` dead, so New MSF campaign's h2s touch their grids and the Evidence fieldset. Flow 09 hit the same trap (`.groups-intro`) | Keep only `.msf-page > :not(dialog)` (or `:where()`); a stylesheet test that the titles keep their margins; the replay checks 1280 and 390 |
| D2 | M | The campaign list is `DataTable Stack` only (`CampaignsList.razor:63`): at 641–900 px a 7-column table with up to three buttons a row, which DESIGN.md § Table system says splits names; L2/L5 at 768 draw both stack classes, and DESIGN.md (`:1018`) says such a list is written by hand, as `MyMsfReports.razor:58` is; `:2184` calls it "stacked" | Hand-write it with both stack classes, as `MyMsfReports` |
| D3 | M | Gendered words for every trainee: `MsfReportText.cs:121,122,206,223,230,240,342` ("no longer on her curriculum", "She can no longer open MSF reports", "her record") | Name the trainee; tests follow |
| D4 | L | `MsfRespond.razor:158` keeps `maxlength="4000"`: a paste over 4,000 is cut silently though the handler refuses in words (`MsfCampaignRules.cs:873`, which prints "4000") | Drop `maxlength`; "4,000" in the refusal |
| D5 | L | `NewCampaign.razor:97`: with no template the select is natively `disabled` | Name the exception in DESIGN.md (a select whose one option is a placeholder) |
| D6 | L | `CampaignEdit.razor:~705`: a refused teaching context leaves the focus on Add invitee (no `@ref`) | Focus the first invalid field |
| D7 | L | `CampaignEdit.razor:121` the open refusal above the buttons, `:177` the withdraw refusal below them | One side |
| D8 | L | Dead CSS: `.account-form-container--wide h2` (`app.css:798`); `.details-grid--lone-spans` (`:2180`, pinned by `NarrowLayoutTests.cs:259`, DESIGN.md `:3384`) | Delete; test and DESIGN.md follow |
| D9 | L | `.msf-page td .actions-cell` (+ `.btn`) twice at ≤640 px (~5890, ~6150); off-scale `gap: 0.75rem`, 2px/0.2rem/0.3rem nudges | One copy in the shared section; the spacing tokens |
| D10 | L (P) | `ProgrammeCoverage.razor:38`: a six-sentence intro V1 does not draw, repeating the subtitle | Two sentences |
| D11 | L (P) | `CampaignsList.razor:49` withdraw success plain; `CampaignEdit.razor:36` bold | Agree with L4 |
| D12 | L | DESIGN.md's account right but for D2, D5, D8; its class list (`:4512`) omits `.msf-page td .actions-cell` | With the items above |

## G: the runbook, the cast and regressions

**Sound:** every figure recomputed from the act file's ratings: Consultant (Khumalo, Botha, van Wyk, each 4) 4.00 from 3;
Nurse (Mokwena 4, Tau 5, Nkosi 4) 4.33 from 3; Peer doctor and Allied health one answer each, suppressed; 8 of 9; 2/1,
3/3, 3/3, 1/1; 3.41's "3 responses, of the 5 needed." and "0 respondent groups reporting, of the 2 needed."; 3.46's 106
characters. The Expects at 3.34–3.49 match the words classes, the group order and `NavOwners`' trails; 3.55's nineteen
commands and 3.57's eight submissions; the outcome SQL; coverage.md's two new routes; 12 presses + 18 fields with 3.36's
Do order; states.md's quotes. Nothing in Acts 2, 4, 5, 6 or other flows' Expects contradicts the flow 10 code.

| # | Sev | Finding | Fix |
|---|---|---|---|
| G1 | L | 3.36's Note: "nine addresses, the group changed on seven adds … and on the last" sums to 17, not 18 (ten address entries, eight group changes); `10-msf-campaign.md:26` the same | Reword |
| G2 | L | states.md "An address refused" says "before the last add"; `naledi.sebego@kgk` is Ms Sebego's, the 7th | "when adding Ms Sebego" |
| G3 | L | `README.md:154` "nine colleagues with no account": Dr Khumalo and Dr Botha hold accounts | "who answer without signing in" |
| G4 | L | Stale Actuals and Gaps at 3.34–3.50 (T270's colon, T325's time, F-3.50a's double refusal; 7 invitees, 6 responses) | The replay rewrites them |
| G5 | L | Stub-mail counts shift by +3 (nine invitations, the release mail): Act 3's outcome 14 (34 after), Act 4's Actuals | The replay prompt says so |
| G6 | L | The pronoun sentences (D3) are quoted by no runbook line; their tests are `CampaignReportPageTests.cs:269–306`; "her" also in `MsfDtos.cs:93` and the brief | With D3 |
| G7 | L | `appendix-cross-cutting.md:1262`: a run-on line over 200 characters | Re-wrap |
| G8 | L (P) | states.md has no recipe for K13, K14/R10, R6b/R6d/R7b, K15, the loading and failed reads | Kept: scratch-only states are taken by the states pass at step 7 |
| G9 | L (P) | The F5 "closed by itself" recipe sets `ClosesOn = current_date - 1` (the server's day) before the job, which judges the South African day | `current_date - 2` |

The runbook lane also noted: the report's Close and Release refusals are `Alert Kind="danger"` with no `Role="alert"`
(the campaign page's have it); `NewCampaign`'s `[Range]` repeats "Enter a number from 3 to 100." instead of
`MsfCampaignText.MinimumInGroupRange`; the release mail's stub log names no recipient (T282), so 3.46 checks only its
subject and body.

## The fix pass

- **Code:** R1 (classify refusals; the missing id a fixed sentence), R2 (one sentence, `MsfCampaignText.ActionFailed`),
  R3, R5, R6 (with a handler test), R8; D1, D2 (hand-written, both stack classes), D3/G6 (the trainee's name, no
  pronoun), D4 (no `maxlength`; "4,000"), D5 (named in DESIGN.md), D6, D7, D8, D9, D11, D12; the Close and Release
  refusals' `Role="alert"`; `[Range]` from the constant.
- **Kept:** R4 (nothing is live; the replay regenerates the data), R7 (the operator), D10 (coverage's opening was
  "as built" by round 1's decision), G4, G5, G8 (step 7).
- **Docs:** G1, G2, G3, G7, G9.
