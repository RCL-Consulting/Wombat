# Curricula

Curricula (`/admin/curricula`, `Pages/Admin/Curricula/CurriculaList.razor`; its words `CurriculaWords.cs`; the reads `GetCurriculaListQuery` (with `CurriculumDto.Institution`, a version's state for one institution) and `GetAdoptableCurriculaQuery`, the adoption `AdoptCurriculumCommand`, `AdoptionRefusedException`; the addresses `Components/Shared/CatalogueLinks.cs`): **the College's curriculum versions, and for an institution which of them it trains against and which it may adopt.** Flow 15 (T392, 2026-11-02, c931d56f; boards R3-A01 to A15, R2-A12; Q1, Q6, Q7, Q10, Q18, Q21, S1, S2, S7, S15, S19, F5, T9; build review C4, P1, P8, P9) made it the one page for adoption. Until then an institution adopted on a page of its own, Curriculum adoptions, from a picker that offered the version it had already adopted (which Adopt then refused), superseding without asking and printing the exception's text on a failure; Curricula printed the College's `IsActive` as "Active", so KGK's superseded 11.1 read "Active".

`/admin/adoptions` (`Pages/Admin/Adoptions/AdoptionsList.razor`) is no menu item since flow 15: it keeps its policy, sends to `/admin/curricula` replacing itself in the history, and draws nothing (its tab reads "Curricula · Wombat" for the moment before it does). NavOwners lights Curricula for it.

## Who reads it

`[Authorize(Policy = "NationalCatalogueAccess")]`; NavMenu: Curricula (`book-open`) in the Catalogue group of the Administrator and the College admin and the Curriculum group of the Institutional admin. It is a list, so it lights itself and has no trail. **What it draws depends on the reader**, never on a parameter:

- **An Institutional admin** (Prof Nolwazi Mbatha at Kgosi Kgari Teaching Hospital): Curricula you train against, then Open for adoption.
- **A College admin** (Dr Anton Kruger, CPSA): the College's versions in one table, no section heading.
- **The Administrator**: All curricula, then An institution's curricula.

## The page

- **The header**: h1 and tab "Curricula". The subtitle says whose: "The College curricula Kgosi Kgari Teaching Hospital trains against, and the versions open to it." ("The College curricula your institution trains against, and the versions open to it." before the name is read); "Manage curriculum versions and the sub-speciality they apply to." for the College admin; "Every College's curricula; and, for one institution, what it trains against and may adopt." for the Administrator. **Header actions** only for those the target page admits (T211): Create curriculum (`.btn-primary` with `plus`, the plus kept as built, round-3-check decision 4) for the College admin and the Administrator, and before it the Administrator's outline "Curriculum progress" (the rebuild, ProgressRebuild).
- **The column**: `<div class="people-page curricula-page">` (flow 12's list column and its phone rules), `aria-busy` while the first read runs. Each section is `section.curricula-section` (a flex column, its heading, result, notice and table 16px apart; the sections 24px apart in `.people-page`'s column), named by its heading.
- **One result a page** (A02, A05): `ActionResult` at the column's top, a success `Alert` whose first sentence is bold: "**Paediatric EPA Curriculum (11.2) adopted.** New admissions to Paediatrics / Paediatrics join it. Paediatric EPA Curriculum (11.1) is superseded; its 3 registrars stay on it until each is moved." (for none on it: "… is superseded."; one: "its registrar stays on it until moved."; a first adoption ends at "join it."). To the Administrator the admissions are named for the institution ("New admissions to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital join it."). An adoption made whose list could not be read again keeps its result and adds "The list could not be read again: reload the page to see it." (P1).

### An institution's two sections

- **Curricula you train against** (`h2#cur-train-h`): a `DataTable` with `StackWide`, captioned "The curricula your institution trains against, with the state of each". Columns: **Curriculum**, the row header, "Paediatric EPA Curriculum (11.1)" (`.col-wrap--wide`); **College**; **Discipline**, "Paediatrics / Paediatrics"; **Effective**, the ISO day; **Items**, "15 national · 1 your own"; **For your institution**, the version's state in words, never a badge (Q10): "Adopted on 2026-11-02", or "Superseded on 2026-11-02" over `div.muted.text-sm` "3 registrars still on it" ("1 registrar still on it"; nothing for none, S2); and one outline **Items**, named "Items of Paediatric EPA Curriculum (11.1)" (the sub-speciality added where two versions share a name, `RowNames.Distinct`). No Edit: the College's versions are the College's. Empty: `.detail-card--empty` "No curricula yet" over "Your institution has not adopted a curriculum yet. Adopt one below before admitting trainees."
- **Open for adoption** (`h2#cur-open-h`): its own result region (a refusal lands here, A07), then the standing notice `p.muted` "Adopting a version makes it the one that new admissions to its discipline join. Adopting another version of a discipline your institution has adopted supersedes the current one: registrars already admitted stay on theirs until each is moved, and your institution's own items are not carried over.", then a `StackWide` table captioned "College curriculum versions open for adoption": **the open versions the institution has not actively adopted, and only those** (Q7), so Adopt is never offered for what it would refuse; a held-back version is not here (T301). Columns: Curriculum, over A2's line in `div.muted.text-sm` ("Newer than the version your institution has adopted (11.1).", "Older than the version your institution has adopted (11.2).", or "Your institution has adopted no Paediatrics / Paediatrics version yet."); College; Discipline; Effective; Items (the national count); and one outline button. Nothing open: `p.muted` "No other College curriculum version is open for adoption."
- **Adopt or Adopt…** (Q6, N6): a first adoption of a discipline supersedes nothing and asks nothing, so its button reads **Adopt** and adopts on the press. Any other, a newer version or going back to an older one (Q21), reads **Adopt…** (`aria-haspopup="dialog"`) and confirms first. Each is named for its row: "Adopt Paediatric EPA Curriculum (11.2)…" (the "…" kept at the name's end). It is never natively disabled: while an adoption runs it is `aria-disabled`, and a second press sends nothing (T202, T234).

### The superseding dialog

The page's one `ConfirmDialog` (action mode, opening on Cancel; flow 14, P5), with `BodyContent`:

- Title: "Adopt Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital?"
- "It supersedes Paediatric EPA Curriculum (11.1) for every new admission to Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital."
- "The 3 registrars on 11.1 stay on it until each is moved on their profile." ("The registrar on 11.1 stays on it until moved on their profile."; left out for none, S1.)
- "Your institution's own item on 11.1, KGK-001, is not carried over: add it again on 11.2." (several: "own items on 11.1, KGK-001 and KGK-002, are not carried over: add them again on 11.2."; to the Administrator "Kgosi Kgari Teaching Hospital's own item …"; left out for none).
- Cancel, then the primary "Adopt 11.2". Cancel or Escape returns the focus to the row's Adopt….

**The adoption carries the version the dialog named as superseded** (C4): if another adoption was made since the page was read, the command refuses rather than supersede what the dialog did not name.

### Refusals and failures (A07, T329)

In Open for adoption's own result region, a danger `Alert` with its first sentence bold, focused, the list read again under it: "**This curriculum version is not open for adoption.** Its College has held it back, so nothing was adopted." (T301: held back since the page was read); "**Another adoption was made since this page was read, so nothing was adopted.** The list below has been read again." (C4); "**This institution has already adopted this curriculum version.**"; "**The selected curriculum could not be found.**"; anything else "**The curriculum could not be adopted.** Nothing has changed. Try again, or come back in a few minutes.", the cause to the log. Never the exception's text.

### The College admin's and the Administrator's lists

- **The College admin**: one `StackWide` table, no heading, captioned "College of Paediatricians of South Africa's curriculum versions". Columns Curriculum, Discipline, Effective, Items (the count), **Status** ("Open" or "Held back": the College's flag is its release for adoption, T9, F5; a new or cloned version starts Held back), and Edit (where `CanEditCurriculum`, T211) and Items, each named for its row ("Edit Paediatric EPA Curriculum (11.2)", "Items of Paediatric EPA Curriculum (11.2)").
- **The Administrator**: **All curricula** (`h2#cur-all-h`), the same table with a College column, captioned "Every College's curriculum versions, with the College's status. Items counts each institution's own items too." Empty: "No curricula yet" over "Create a curriculum before admitting trainees or building progress rules." (the College admin's too).
- **An institution's curricula** (`h2#cur-inst-section-h`; Q18, P4, R3-A09 to A11): `form.search-container` (`novalidate`) on `.search-grid`, **Institution** (`label for="cur-institution"`, `select#cur-institution`, its first option "— Select an institution —") and an outline **Show** (the form's submit, so Enter is Show). The choice is the page's state, not the address, and is kept across an adoption. Until Show has read, `p.muted` "Choose an institution, then Show, to see what it trains against and adopt for it." Show with none chosen reads nothing and leaves what is shown: "Choose an institution." under the select (`div.validation-message role="alert"`, the select `aria-invalid` and described by it), the select focused (P8). Show's answer is the institution's two sections as h3s named for it, "Curricula Kgosi Kgari Teaching Hospital trains against" (`h3#cur-inst-h`, which takes the focus) and "Open to Kgosi Kgari Teaching Hospital", the state column "For this institution", its notice in the institution's words ("… a discipline the institution has adopted … the institution's own items are not carried over."), its empty body "Kgosi Kgari Teaching Hospital has not adopted a curriculum yet. Adopt one below before admitting trainees.", and every Items and Adopt named for it ("Items of Paediatric EPA Curriculum (11.2) for Kgosi Kgari Teaching Hospital", "Adopt Paediatric EPA Curriculum (11.1) for Kgosi Kgari Teaching Hospital…"; S15).

## States

- **Loading** (S19): `p.loading-line[role="status"]` "Loading the curricula.", then, for an institution and the Administrator, each section's heading over a skeleton card; no Adopt until the read is in. The College admin's is the line alone.
- **Load failed** (S7, R3-A15): "Could not load the curricula. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it reads; no empty state under it. The cause goes to the log (T329).
- **Show failed** (P9): inside the section, an `ActionResult#cur-inst-error` holding a danger `Alert` with `.alert-row`, the same sentence and an outline "Try again" with `refresh-cw`, which reads the chosen institution again; the failure takes the focus. Show while it reads: the loading line.

## Reads and focus

The first read and Try again draw the loading state; an adoption, Show and Try again in the section answer in place. An adoption's result takes the focus (`ActionResult`, at the top); a refusal or failure takes it in the open section; Show's answer focuses its first h3; a refused Show focuses the select. Every adoption, refused or not, reads the list again, the Administrator's chosen institution kept.

## Narrow widths

Every table is `StackWide`, so it stacks at 900px and below, a block per version: the curriculum first (with A2's line under it in Open for adoption), each other cell labelled ("Discipline:", "Effective:", "Items:", "Status:", "For your institution:"), its buttons last. Below 641px `.people-page` makes every button, input and select 44px, and the filter's Show is the row's width (`.filter-actions .btn`); the dialog's buttons stack with Adopt 11.2 on top (`.dialog-card .form-actions`, the system rule since flow 11). app.css's "Flow 15: curricula" holds only `.curricula-section`'s column and its spacing (`.action-result:empty` takes no row; the chosen institution's sections 8px further apart); nothing else is needed. Measured on the replay at 390 (A.7.10: three stacked blocks), 1280, and 901 to 1100: nothing sideways.

## Contrast

No new colour token. A2's line, "‹N› registrars still on it", the notices and the hint in `muted-text`, 5.09:1 on the surface (4.86:1 on the page's ground); the result and refusal as Alert's.

## Known gaps

- The Administrator's Institution form carries no accessible name of its own (Data rights requests' filter is `aria-label`led); its select and Show are labelled.
- An adoption whose read-back fails says so in words only; nothing offers a Try again for the list (P1, by design: reload).
- `CurriculumEdit.razor` (flow 16's create and edit page, which this page links) still prints `exception.Message` at three places (build review P6, left for a task; T393 for the same on two edit pages).
