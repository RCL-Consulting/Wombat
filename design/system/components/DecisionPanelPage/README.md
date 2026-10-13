# DecisionPanelPage

A decision panel's own page (`/committee/panels/{PanelId}`) and New panel (`/committee/panels/new`), one Razor page, `Pages/CommitteeDecisions/PanelEdit.razor`, its words in `Panels/PanelWords.cs`. Flow 08 (T364, 2026-10-13, 622c5ad1; boards R2-2; Q5, Q6, Q8; C10–C13; round 3 M6, M7, F5, S3, S4, S6, S7; T260, T324, T329, T338) rebuilt it: the page is **titled by the panel**, says what cannot change, seats people by person (SeatPicker), and explains why Decides for is locked. Until then it was one form headed "Edit panel", with two native multi-selects for members and externals and a "Back to panels" link.

## Who reads it

Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`), for the panels they manage. **Not the Coordinator or the Committee member**: the page does not admit them (Access denied), and the panels list is where they read the panels (DecisionPanels). It lights Decision panels (NavMenu); the trail is Home › Decision panels › the panel's name (Breadcrumbs).

## A panel's page

`div.committee-calendar`. There is no "Back to panels": the trail is the way back (Q6).

- **The header**: the h1, the tab and the crumb are the panel's name, "Paed Annual Review Panel"; the subtitle what it covers, "Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital" ("Institution panel · …" for an institution-wide one).
- **Created** (`?created=1`, only "1"): "**Paed Annual Review Panel was created.** Its reviews can now be scheduled on Committee reviews." (`alert-success`, `role="status"`), focused. Save panel's and Save committee's results take its place and the focus: "**Panel members updated.** Sarah Botha is off the panel. Thandi Zulu chairs, with 1 member and 1 external member." (C12); "The panel sits as the Neonatal team Clinical Competency Committee." / "The panel is a general panel.".
- **About this panel** (`section.detail-card.panel-section`, h2): a `dl.details-list`, Scope "Speciality: Paediatrics" (or "Whole institution"), Institution, Open reviews "None" or "Review #1 (2026 S2, scheduled) and 4 more", the first a link where the reader may open it; under it, `.muted`, "Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its reviews are open." (Q8).
- **Who sits on it** (`form.form-container.panel-section`, h2 `.form-section-title`): the refusal summary when there is one; a warning Alert (`role="alert"`) naming whoever can no longer sit, "Sarah Botha can no longer sit on this panel, so is not listed below: only an active committee member at its institution who is not a trainee can. Saving takes Sarah Botha off the panel." (M6), read again from the saved panel after Save panel (C12); the seat picker (SeatPicker); Save panel (`btn-primary`, "Saving…" while it runs).
- **Decides for** (`section.form-container.panel-section`, h2): which College committee the panel sits as, offered three ways:
  - **An Institutional admin or an Administrator**, nothing open: a select ("General panel", then each College committee) with its help ("A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the panel is scheduled, in progress or awaiting ratification."), and Save committee.
  - **While a review before the panel is open** (F5): no select; the value as text (`p.readonly-value`), then an info alert (`.alert.alert-info.panel-lock`), "**It cannot change now.**" and the refusal's own sentence ("Review #1 (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, …"), and "Open review #1" under it where the reader may open it.
  - **A speciality admin**: the value as text, and "Only an institutional administrator can change which College committee a panel sits as." (`.muted`).
  - **A change refused by a review scheduled meanwhile** (a second tab, C13): the section's own summary, "Not saved. The panel is still a general panel.", its line "Decides for: review #3 now sits before this panel" as a link to the select, then why; beside the select, "Not saved: review #3 (2026 S2, scheduled) now sits before this panel. The select shows what is saved." The select goes back to what is saved.
- **Both saves are never natively disabled** while either runs (`aria-disabled`; T234, build review D3): the pressed one keeps the focus, and a second press sends nothing.

## New panel

The h1 and tab "New panel", the subtitle "A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and speciality cannot change once it is created." One `form.form-container` named "New panel": About this panel (`h2.form-section-title`) over a `.form-grid` of Panel name (required), **Scope** (radios, `fieldset.form-group.choice-row` with a `.check-grid`, Institution and Speciality; or, where one scope is offered, a `dl.form-group` saying it as text with why, "A speciality administrator creates Speciality-scoped panels for the speciality."), Institution (for an institution-wide panel, and an Administrator's every panel), Speciality ("Choose a speciality", with its help) and Decides for (an Institutional admin's and an Administrator's); then Who sits on it (the seat picker; the second `.form-section-title` stands 32px clear of the fields); then Cancel (to Decision panels) and Create panel. Created, the page goes to the panel's own page with `?created=1`.

Someone who may create no panel is offered no form: the info Alert "You cannot create a decision panel. …" and "Go to Decision panels" (T194, T260). Someone who holds Trainee reads the trainee note and the same link (T256).

## Refusals (C7, T324, S4)

A refusal is said once in a summary at the top of its form (`.validation-summary-errors.error-summary`, `role="alert"`, focused once drawn): "**The panel was not created.** Everything you chose is kept below." ("The panel's members were not saved."), then one line a field, each a link that moves the focus to its field ("Chair: Choose a chair.", "Seats: …"), and again under each field (`.validation-message`, the control marked invalid). A handler's own refusal (`CommitteeRefusedException`) prints its words; any other failure is "The panel could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.", logged (T338, build review R4).

## States (C11, M7)

- **Loading**: the h1 "Decision panel", a visually hidden status "Loading the panel.", skeletons.
- **Load failed**: "Could not load this panel. Nothing has changed. Try again, or come back in a few minutes." ("Could not load the new-panel form. …") with Try again; the h1 stays "Decision panel".
- **Not found** (an id out of the reader's reach, or unknown): flow 01's Page not found, word for word (M7): the h1 "Page not found" with PageHeader's `search` icon, "There is no page at this address.", "Check the address, or start again from Home." and "Go to Home" (`home`); it lights nothing (PageDrawn).

## Narrow widths

Below 641px every control on the page's forms is 44px (`.committee-calendar .form-check`, `.form-control`, `.form-select`), and the form's actions are 44px and the row's width. The seat picker stacks (SeatPicker).

## Contrast

No new colour token. `.readonly-value` is body text at weight 600, 12.63:1; About's labels and the rule `muted-text` 5.09:1 on the surface; the lock's words 11.84:1 on `info-bg`, its link 6.28:1; the warning Alert's words 11.89:1 on `warning-bg`.

## Known gaps

- None the replay met on this page (T260's naming and lock no longer occur, Steps 2.22 to 2.25 and 4.13). T260's other half, the STAR revocation's button, is not this page's.
