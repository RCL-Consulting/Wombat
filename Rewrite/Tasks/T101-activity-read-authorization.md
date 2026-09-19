# T101 — Any authenticated user can read any activity's full DataJson by id

**Status:** open
**Surfaced:** 2026-09-17, mapping the activity transition pipeline for T070.
**Severity:** High (confidentiality) — WBA data is clinical performance data about a named trainee.

## Symptom

`GET /activities/{id}` returns any activity in the database to any signed-in user, including another
trainee's. There is no institution, speciality, subject or assessor check anywhere on the read path.

## Root cause

- `src/Wombat.Application/Features/Activities/Queries/GetActivityById/GetActivityByIdQuery.cs:7-19` —
  the request record is `(int ActivityId)`. It carries **no `ClaimsPrincipal`** and the handler calls
  `IActivityService.GetAsync` with a bare id.
- `src/Wombat.Web/Components/Pages/Activities/ActivityView.razor` is gated by a bare `[Authorize]` —
  authenticated, no policy, no ownership check.

Contrast with the T056 convention documented in `CLAUDE.md`: handlers reachable from scoped pages take
`ClaimsPrincipal Principal`, lists filter on `principal.GetInstitutionId()`, and get-by-id calls
`principal.CanAccessInstitution(...)` returning **null (404, not 403)** when out of scope, so the existence
of another institution's ids is not leaked. The activity read path never adopted it.

## What "authorised to read" should mean

An activity is legitimately readable by, at least:

- the **subject** (`Activity.SubjectUserId`) and the **creator** (`CreatedByUserId`);
- the **bound assessor** — the value of whatever field the workflow's actor rules point at
  (`field:assessor_user_id` for the CPSA seeds). Note this is data, not a column, so the check has to read
  `DataJson` the way `WorkflowEvaluator.cs:84-107` does;
- **Coordinators, CommitteeMembers, SpecialityAdmin / SubSpecialityAdmin, InstitutionalAdmin** within the
  activity's scope, and a global **Administrator** everywhere.

Deciding that list is the substance of this task; the plumbing is mechanical.

## Note for whoever does T070 first

T070 adds a `ClaimsPrincipal` to `GetActivityByIdQuery` for a *different* purpose — computing which fields
the caller may edit. **That is not a read gate**, and the T070 plan adds an explicit code comment saying so.
Do not read the presence of the principal as evidence this task is done.

## Verification

Sign in as trainee A, request `/activities/{id}` for an activity belonging to trainee B in another
institution: expect a 404-equivalent (not found), not the activity. Repeat as the bound assessor (allowed),
as an unrelated assessor (denied), as a Coordinator in scope (allowed) and out of scope (denied).

## Related

Sits directly under the page [T070] rebuilds. Same family as [T102].

---

## Design decided — 2026-09-19

Three designs were produced independently (minimal identity-only, role-complete, actor-grammar-DSL) and
judged by three adversarial lenses (correctness, production lockout, house convention). The panel split
2–1 for the minimal design, but **on a premise that turned out to be false**: every lockout argument
priced the loss of live clinical records. The maintainer confirmed nothing in Wombat is live — dev and
`wombat.rcl.co.za` both hold regenerable scenario-replay data. See the CLAUDE.md section
"🚨 Nothing is live — compatibility is not a constraint".

With compatibility off the table the ranking inverts, because the minimal design's headline merit was
that it broke nothing. What ships is the design the panel ranked *last* on grounds that no longer apply.

### `Activity` carries its own scope

`Activity.InstitutionId` / `SpecialityId` / `SubSpecialityId`, nullable, **stamped at creation** from the
subject's `TraineeProfile` (`ActivityService.ResolveSubjectScopeAsync`). Stamped rather than derived so
that authorization is a column comparison rather than a three-table join per read, and so a trainee
transferring institutions does not retroactively move the visibility of assessments written about them
elsewhere. Null when the subject has no profile, and null satisfies no scoped arm — fail closed.

The migration backfills from `TraineeProfiles` using the identical derivation and tie-break, so a
backfilled row and a row created a minute later are indistinguishable.

### `scope:` now resolves from the activity, not from the activity type — this is a behaviour change

`ActorRuleMatcher.IsInActivityScope` compared the caller's claims against **`ActivityType.ScopeId`**,
which is the scope that decides who may *offer* the tool, not who the assessment is *about*. The two
diverge in the dev data today: activity 11 is a type scoped to speciality 3 about a subject in
speciality 2. The consequence ran backwards — `role:SpecialityAdmin+scope:speciality` matched an admin
of the tool's speciality, who has no relationship to the trainee, and did not match the admin who
actually oversees them. Oversight follows the trainee, so it now resolves from the stamp.

All six `scope:` tokens in the entire seed corpus are `role:SpecialityAdmin+scope:speciality`, so this
changes those six rules and nothing else. `scope:global` still reads the type, because it asks whether
the tool itself is unrestricted.

### The readable set

`ActivityService.IsReadableBy`, a superset of the act set by construction:

1. global `Administrator`;
2. the subject; the creator;
3. any prior transition actor — an assessor who declined keeps sight of their own decision even after
   the trainee re-points `assessor_user_id` at someone else;
4. scoped oversight: `InstitutionalAdmin` / `Coordinator` / `CommitteeMember` on `InstitutionId`,
   `SpecialityAdmin` on `SpecialityId`, `SubSpecialityAdmin` on `SubSpecialityId`;
5. anyone named by **any** actor rule the pinned version declares anywhere — transition `actor`, state /
   section / field `editable_by` — with the state gate dropped, asked through `ActorRuleMatcher` so the
   read gate and the act gate cannot drift.

Clause 5 deliberately **ignores an unqualified `role:` token**. Unqualified, it would turn "a Coordinator
may approve this" in one seed into a grant to read every activity of that type in every institution — a
read boundary widened by an unrelated seed edit. A role token counts only when conjoined with something
binding it to this activity, which is what every rule in the corpus does today. Role-only oversight is
granted by clause 4, where the scope is explicit.

### Refusal

`GetDetailAsync` returns **null** for "no such activity" and "not yours" alike, per the T056 convention;
`ActivityView` renders the existing "Activity unavailable" empty state. `GetAsync` — principal-less, zero
callers, and a standing invitation to bypass the gate — was **deleted** rather than gated.

An unreadable pinned version inside clause 5 is caught and treated as a refusal, so an unauthorized
caller cannot learn that an id exists and its schema is broken. A caller admitted by clauses 1–4 still
gets the real exception, because clause 5 runs only after those decline.

### Scope of the commit

The panel found that the activity read path was not the widest door. Closed in the same commit:

- **the audit log** — `CreateActivityCommand.InitialDataJson` and `UpdateActivityDraftCommand.NewDataJson`
  reached `SummaryJson` unredacted while the sibling `TransitionActivityCommand` already redacted exactly
  this data; command audit rows carried `InstitutionId == null`, which `ListAuditEntriesQueryHandler`
  shows to *every* `InstitutionalAdmin`;
- **the portfolio PDF export** — `ExportPortfolio.DemandExportAccess` allowed five roles with no scope
  comparison at all, and the PDF renders raw `DataJson`;
- **`ListActivitiesBySubjectQuery`**, **`GetEpaTrajectoryForTraineeQuery`** and the coordinator
  dashboard's stalled-activity query, all of which returned activity data filtered by a caller-supplied
  id or by nothing.

---

## Adversarial review — 2026-09-19 — and what it changed

Four review lenses (bypass-hunt, wrongly-refused, logic/data, drift/convention) over the finished diff,
each finding then adversarially verified by a separate agent instructed to refute it. Eleven findings
survived. Four were defects **in this work** and were fixed before commit; the rest are pre-existing and
are now filed.

### Fixed here

- **The speciality arms were national.** `Speciality` carries a `CollegeId` — it is a College-owned,
  country-wide id — so matching `IsInSpeciality` alone let one hospital's SpecialityAdmin read, list and
  **export the complete portfolio PDF** of every trainee in that speciality nationally. Wider than the
  hole T101 was filed to close, and three of the new tests pinned it as correct. Every scoped arm now
  conjoins the institution, at all four gate sites plus the `scope:` grammar. CLAUDE.md always said
  "SpecialityAdmin — scoped to one speciality **within an institution**"; it takes both claims to say so.
- **A null stamp froze an activity for ever.** `InvitedUserProvisioner` gives an invited user speciality
  scopes at acceptance while `AdmitTrainee` creates the `TraineeProfile` later, and nothing stops them
  filing a reflective note in between. Its only transitions are `role:SpecialityAdmin+scope:speciality`,
  which a null stamp matches for nobody — and neither `WorkflowEvaluator` nor `TransitionAsync` has an
  Administrator bypass. Fixed at the source: `ResolveScopeFromIdentityAsync` falls back to the subject's
  own identity record, which is where the login claims come from anyway. The residual case (no
  institution at all, or several speciality scopes) is [T116].
- **The guard against unqualified `role:` tokens threw away bound arms with them.**
  `field:assessor_user_id|role:Coordinator` parses as one `Any` node, and the old check discarded the
  whole rule — so the named assessor could act, saw the row in their inbox, and got "Activity
  unavailable" on opening it. Exactly the buttons-that-404 failure the gate exists to prevent, caused by
  the guard against it. Now rewritten per ARM: `Any` keeps its qualified arms, `All` is kept only if
  something in it binds to the activity. `role:X+scope:global` does not, because `scope:global` reads
  the activity *type*.
- **The export gate and the export body disagreed.** Access was decided from the trainee's current
  profile while `PortfolioPdfService` selected on subject and dates alone, so the PDF could contain rows
  the caller is refused individually — and rows filed before the subject had a profile, which no
  overseer may open. `PortfolioExportRequest` now carries the principal and the query runs through
  `WhereReadableBy`. The subject-access-report path passes the **data subject's** own principal, because
  a statutory access request must return everything held about that person whoever operates the export.

Also fixed: the clause-5 catch filtered `JsonException`, which the parsers *wrap* into
`SchemaParseException` / `WorkflowParseException` (both deriving from `Exception` directly) — so it
caught nothing they throw and a malformed pinned version was distinguishable from a nonexistent id.
And a `null == null` hazard: both sides of the institution comparison are `int?`, so an unstamped
activity would have matched a principal with no institution claim.

### Refuted, and worth recording so they are not re-raised

- The migration backfill's INNER JOIN cannot drop rows the C# would stamp: `TraineeProfile.CurriculumId`
  and `Curriculum.SubSpecialityId` are non-nullable with required FKs, so a dangling reference is not
  representable.
- `LoadActivityAsync` staying ungated is not reachable: `TransitionActivityCommand` has two dispatch
  sites, both already behind the read gate, and `Wombat.Api` maps only `/msf/respond` and `/health`.
- The architecture test scanning only the Application assembly is a coverage boundary, not a defect —
  `WhereReadableBy` is an Application extension that Infrastructure services neither do nor should call.

### Filed, not fixed

[T112] data rights (wider than this hole, needs a product decision), [T113] two remaining
caller-supplied trainee ids, [T114] audit hygiene, [T115] the dead `IsInRole` extension, [T116] the
unstampable activity, [T117] the weekly digest's national roster.
