# Flow 01 · Round 2 review (2026-09-27)

Four reviewers (tokens, shell, states, build plan) and a synthesis, read against `../01-shell.md`, `../../BRIEF.md` § 4–§ 6 and the code. The decisions D1–D11 are the operator's.

**Round 2 of the Flow 01 shell: synthesis of the four reviews**

**Verdict: accept with changes.** All four reviewers (Tokens, Shell, States, Build) reached the same verdict. The frame follows the operator's pick, A with C's breadcrumbs on detail pages, and it meets the brief's structural requirements. The problems are rules left unwritten or contradicting each other, states that cannot occur or cannot be built as drawn, and gaps in the token contract.

**Blocking: none.** Step F still cannot start until the operator answers D1, D2, D6, D8, D9, D10 and D11 and the canvas returns round 3.

What I checked in the repo myself (no file changed):
- The pilot README's constraints say "Sign-in, sign-out and the other cookie writes stay form posts". Its Run section tests only Web and Architecture.
- `design/flows/01-shell/README.md` records that "a deep link switches only to a role the person holds". It also says body text, dark mode and reduced motion "were settled in the round-2 message", but the answers are not written anywhere in the repo.
- BRIEF line 407 and T317's recommendation both want the switch remembered per person.
- `design/flows/01-shell/` is still untracked.

---

## Findings

Each finding says where the fix belongs: the design canvas, the build, or both.

### Should-fix

**S1. Access denied is built around the acting role.** (Denied-In, Denied-Narrow · design · Shell, States, Build)
- The "Switch to Assessor and open" state can never occur, because access is judged on every role the person holds.
- The copy frames access around the acting role.
- The "ask your institution's administrator" line sends an InstitutionalAdmin to herself.
- There is no copy for when no ReturnUrl or matching route is available.
- **Fix:**
  - Remove the switch panel.
  - Word the refusal from the person's claims alone. Two roles: "None of your roles (Committee member, Assessor) opens this page." One role: "Your role (Trainee) does not open this page."
  - The ask line says "the platform administrator" for an InstitutionalAdmin and "your institution's Wombat administrator" for everyone else.
  - See D6 for naming the page.

**S2. The signed-out Access denied is only reached by typing its address.** (Denied-Out · design · States)
- "Brings you back here" returns the visitor to /access-denied itself.
- **Fix:**
  - With a local ReturnUrl, Sign in goes to `/account/login?ReturnUrl=<it>` and the copy says "brings you back to the page you asked for".
  - Without one, the copy is "Sign in to carry on."
  - The tab title matches the h1.

**S3. The signed-out Not found boards show a case that cannot happen.** (NotFound-Out, NotFound-Narrow · design · States)
- A signed-out visitor at an unknown address is sent to sign in first (A.5.4). The only real entrance is typing /not-found.
- **Fix:** design for that entrance. Show no echoed address and drop the "sign in first" line.

**S4. The Not found copy leaks and echoes.** (NotFound-In · design · States, Build)
- "A record from another institution is reported this way too" undoes the 404-not-403 rule.
- The echoed address lets a crafted link put its own words on a Wombat page, the class of problem T285 closed. It would also flicker between the prerender and the circuit.
- **Fix:** delete the sentence and drop the echo. The address bar still shows the address, which is all A.5.4 asks.

**S5. The error page depends on T321, and its annotation is wrong.** (Error-* · both · States, Build)
- Nothing reaches /Error today: there is no UseExceptionHandler.
- The annotation says the page "becomes interactive". T321's page is static.
- **Fix (build):** fold T321's pipeline work into T335:
  - `UseExceptionHandler("/Error", createScopeForErrors: true)` after SecurityHeadersMiddleware, serving the page only for a GET that accepts HTML;
  - Error.razor gets `[AllowAnonymous]` and `[ExcludeFromInteractiveRouting]`;
  - Try again's link comes from `IExceptionHandlerPathFeature.Path` plus the query string, local paths only.
- **Fix (design):** reword the annotation to "static, rendered once inside the failed request".

**S6. The error page's reference and missing states.** (Error-* · both · States)
- The reference drawn is the 55-character traceparent. An administrator cannot search the journal for it, and it wraps at 390 px.
- /Error typed directly (step A.5.8) has no design.
- The signed-out copy assumes an account, but an MSF respondent can land here.
- **Fix:**
  - Show the 32-hex trace id (`Activity.Current?.TraceId.ToHexString()`, falling back to `TraceIdentifier`), and log that same value with the exception.
  - Add the typed state: no reference and no Try again, only Go to Home.
  - Signed-out copy: "send this reference to whoever sent you the link, or to your Wombat administrator."

**S7. The shell's data and names.** (all signed-in boards · both · Shell, States, Build)
- The error page draws the full shell. If the shell reads the name from the database, the page fails in a database outage.
- The boards print "Dr" (no title is stored), "devadmin", and a trainee who is not in the cast.
- **Fix (build):**
  - Add a display-name claim in WombatUserClaimsPrincipalFactory, falling back to the email.
  - The shell reads no data: roles, the name and the acting role come from claims and the cookie only.
- **Fix (design):**
  - Print FirstName LastName ("Thandi Zulu").
  - The Administrator is "Demo Administrator"; the no-name bootstrap admin shows a truncated email.
  - Use cast rows, such as Nomsa Mahlangu's Mini-CEX.

**S8. The switch rule contradicts itself and cannot be built as drawn.** (Detail-Email, Landing-Assessor, Denied-In · both · Shell, States, Build)
- Detail-Email switches role automatically on a page both roles admit. A's spec says such a page keeps the current role.
- An interactive page cannot write the cookie.
- "Switch back" has no destination.
- Nothing tells Home to show the switch alert, and FocusOnNavigate would take focus to the h1 instead.
- **Fix (design):** write one rule on the canvas:
  - access is the union of held roles and never changes;
  - in-app navigation never switches;
  - only a link that names a role, such as an email's, switches, and only to a role the person holds, through `/dashboard/switch/{role}?returnUrl=<local>`;
  - "Switch back" returns to the same page.
- **Fix (build):**
  - The endpoint accepts a local returnUrl (LocalRedirect only).
  - A successful switch sets a one-time flag. The landing page shows the result alert and moves focus to it.
  - A refused or forged switch sets no flag.
  - See D2 for GET versus form post.

**S9. The active-item rule is unwritten, and NavLink cannot express it.** (Detail-*, failure boards · both · Shell, Build)
- The rule does not cover the T331 routes or My Account.
- In .NET 10 an active NavLink renders `aria-current="page"` itself, and prefix matching cannot light Activity Inbox for /activities/{id}.
- **Fix (design):** an owner table:
  - a list lights itself with aria-current=page;
  - a page under it lights its owner for the acting role with aria-current=true;
  - /admin/colleges/{id}/specialities lights Specialities for a CollegeAdmin and Colleges for an Administrator;
  - /msf/reports/{id} and /msf/coverage light MSF Campaigns;
  - My Account, Change password and the failure pages light nothing, and on My Account the top-bar name carries aria-current=page;
  - retire "lights Home".
- **Fix (build):**
  - a `Navigation/NavOwners.cs` registry;
  - Routes cascades RouteData;
  - a `NavItemLink` component keeps the class `active` and writes aria-current only on the page itself.

**S10. The phone boards miss cases.** (Phone-* · design · Shell)
- No board shows a two-role person's switch at 390 px.
- No board draws the folded bar with a long label such as "Sub-speciality admin".
- The role head is not specified for someone with no role or for a PendingTrainee (see D8).
- The disclosure for three or more roles is not drawn.
- **Fix:** add these boards and state the heads.

**S11. The CSS-only phone toggle.** (Phone-* · both · Shell, Build, Tokens)
- aria-expanded sits on a `<label>`, where it is invalid.
- A label cannot take focus.
- On interactive pages the checkbox stays checked through an in-circuit navigation, so the next page opens under an open menu.
- **Fix (design):** drop aria-expanded, and draw the toggle focused: a white ring on the label, driven by the checkbox's `:focus-visible`.
- **Fix (build):** the checkbox is visually hidden but focusable, rendered first in `.page`, with `@key` set to the path, so each navigation re-creates it unchecked. The label is a 44 px target named "Menu".

**S12. The Administrator's sidebar at 1280 px.** (Shell-Admin, Tokens · both · Tokens, Shell, Build)
- About 709 px of nav against a real viewport of 680 to 720 px. The board hides this with overflow:hidden.
- The outset focus ring spills into neighbouring rows and is clipped at the scroll container's edges.
- A tabindex on the scroll region would add a tab stop to every page.
- **Fix:**
  - Draw the list scrolling under a fixed brand cell and role head, with the first and last items focused.
  - Leave 2 to 4 px between rows and `padding-block` of at least 4 px.
  - The list takes no tabindex.
  - Paint the gradient on a full-height column.

**S13. Every focusable control on the dark chrome needs `--nav-focus-ring`.** (Tokens, Phone-*, *-Out · design · Tokens)
- The page ring on #052767 is 2.90:1, which fails 3:1.
- This covers the brand link, the Menu toggle, nav links, the switch, the account link and Sign out, at 1280 and at 390, and the signed-out gradient bar.
- **Fix:** restate the rule on the sheet and add the phone-bar and signed-out-bar pairs.

**S14. The nav-active-bg swatch is wrong.** (Tokens · design · Tokens)
- The swatch shows #7d8cae. The blend actually paints #62779f at the gradient start, and white on the swatch colour would be 3.37:1.
- There is no token for the white text on the current item. nav-text on the active background is 3.14:1.
- **Fix:**
  - Show the swatch as #62779f and state that the token stays translucent and is valid only over the gradient.
  - Add `--nav-text-strong` (#fff).
  - Record nav-text on nav-active-bg as a forbidden pair.

**S15. Colours on the boards that are not in the token set.** (all signed-in boards · design · Tokens)
- #084a8a for link hover, `rgb(255 255 255 / .18)` for dividers, literal white, three shadow literals, and #1b2f52 as a stand-in for the mark.
- **Fix:**
  - Add `--link-hover`, `--nav-divider` and `--nav-text-strong`.
  - Add shadow tokens: keep `--shadow-color` and add raised, dialog and bar variants.
  - Render the mark as /brand/wombat-mark.svg at 32 px.
  - Mark the canvas-only colours as not carried over: #5b6068, #e6e8eb, the `.slot` stripes and the scroll-fade.

**S16. `.btn-outline` needs a fill.** (Tokens, every `.btn-outline` · both · Tokens)
- The boards give it a white fill, but app.css's is transparent.
- Transparent fails on header-bg (4.37) and success-bg (4.32), and passes by 0.07 on warning-bg (the error bar's Dismiss).
- **Fix:** `.btn-outline { background: var(--surface-color) }`, stated on the sheet and pinned in ContrastTests.

**S17. No motion tokens and no reduced-motion rule.** (Tokens, Landing-Loading, Reconnect · both · Tokens)
- Only board captions promise reduced-motion behaviour.
- **Fix:**
  - Add `--motion-fast` (150 ms).
  - Under `prefers-reduced-motion: reduce`, the skeleton becomes a still header-bg block, the reconnect spinner, bar, slide and fade stop, and transitions go to 0.

**S18. The reconnect dialog.** (Reconnect · both · States, Build)
- The runtime has no call that skips its countdown, so "Try now" cannot be built, and calling `Blazor.reconnect()` races the runtime's own attempt.
- A bare Reload in Failed and Resume failed swaps Wombat for the browser's error page and throws away a circuit the server might still restore.
- Paused invents a trigger: nothing in src calls `pauseCircuit`.
- The rejected state has no design, so an empty box shows until the reload lands.
- Screen readers hear only the first state.
- **Fix (design):**
  - Drop "Try now". When an attempt starts, the line reads "Trying again now".
  - Failed and Resume failed get one "Try again" button. Copy: "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost."
  - Paused: "This page is paused. Resume to carry on."
  - Rejected: "Reloading the page…", with no button.
  - At 390 px the width is 358 px (16 px each side).
- **Fix (build):**
  - Wire Try again to the template's `retry()` and `resume()`.
  - Act only when `detail.state` changes: write the state's sentence (without the countdown) to a polite live region and set the dialog's role.
  - Move focus to the heading on show only.
  - Add the T330 CSS fix.

**S19. The error bar.** (ErrorBar · both · States, Build)
- The runtime sets an inline `display:block`, which overrides the flex row drawn on the bar.
- There is no 390 px layout.
- **Fix:**
  - Put the row on an inner wrapper and keep the `.reload` and `.dismiss` classes.
  - At 390 px: the sentence first, then Reload and Dismiss side by side at 44 px.
  - Use the warning tokens.

**S20. The landing cards.** (Landing-*, Shell-Admin, Shell-Assessor · both · Shell, Build)
- The Assessor's "Pending requests" duplicates "Waiting for your rating" (T297 made them one set).
- The Administrator's "Scheduled jobs" duplicates System health's job row, and "Maintenance" repeats the nav. Its Curriculum progress link has no other way in.
- Card contents belong to later flows anyway.
- **Fix (design):** label the cards as placeholders and drop the duplicates. Keep /admin/curriculum-progress reachable.
- **Fix (build):** keep today's cards and build only the frame and the header action per role.

**S21. Mixed capitalisation, T190's symptom.** (all boards · design · Shell)
- **Fix:** sentence case everywhere. Each nav label, its h1, its breadcrumb and its title stem use the same words ("Activity inbox", "Log an activity", "My data rights").

**S22. Build-only corrections.** (Build)
- **(a) The acting role inside the circuit.** Resolve it once in App.razor from the cookie, the claims and `DashboardPriority.Order`, only ever a held role. Pass it as a `<Routes>` parameter and cascade it with IsFixed, never through IHttpContextAccessor in a circuit.
- **(b) The 1280 grid.** Three 320 px tracks need 1,008 px against 974 available. Lower the track to `min(280px,100%)` or add an explicit three-column rule from about 1,100 px, and keep the collapse at 900 px or below.
- **(c) The landing frame.**
  - DashboardCard gets IsLoading, showing a skeleton inside each card.
  - A load error shows one Alert with fixed copy and Try again, with no cards (T329).
  - Guard the eager view-model reads (CoordinatorDashboard:13, SpecialityAdminDashboard:27).
  - The subtitle's semester is a pure function of today's South African date.
- **(d) The type change reflows every page.** Make it its own first commit and re-check A.7.2 to A.7.12 at 390 px.
- **(e) Deleting PlaceholderPage.** Rewrite the runbook Route lines and coverage.md rows that name it in the same commit, and replace the placeholder test with "no nav link targets /placeholder/".
- **(f) Test runs.** Run all five suites (Domain, Infrastructure, Integration, Web, Architecture), never with `--no-build`.

### Decide (the operator's calls)

**D1. How long the switch is remembered. Needed before step F.**
- Per account across sign-ins (T317, BRIEF line 407; Step 2.34's Expect stays), or session only (A-Spec; Step 2.34 changes).
- **Recommend per account.** The cookie is protected with Data Protection and bound to the account id. Another account ignores it, and sign-out deletes it. This gives the same protection as session-only with fewer places to clear it.

**D2. How the deep-link switch works, and GET versus form post. Needed before step F.**
- The operator already chose that a deep link switches, but only to a held role. An email link cannot POST, and the README says cookie writes stay form posts.
- **Recommend one GET endpoint with `?returnUrl=`, recorded as a W-decision** exempting it from the form-post rule. The write is only a view preference, and unheld roles and non-local URLs are refused.
- Build the endpoint in T335. The role-addressed email links move onto it in flow 19.

**D3. Where the brand sits.**
- R2 keeps the brand cell at the head of the sidebar; A moved it into a full-width top bar.
- **Recommend keeping R2.** It is today's structure, so NarrowLayoutTests hold. DESIGN.md must describe R2, not copy A-Spec.

**D4. The nav-active-bg alpha.**
- At .37, white text is 4.52:1 at the gradient start. At .32 it is 5.27:1, but the fill drops to 2.67:1 against a plain item.
- **Recommend .32,** with the 3 px white bar and weight 600 recorded as the state cue.

**D5. The breadcrumb from a committee review.**
- The trail follows the owning list, so an activity opened from a review has no trail back to the review.
- **Recommend the owning-list trail in flow 01.** Flow 07 decides whether evidence opens inside the review or carries a context trail.

**D6. Access denied: one address or two, and naming the page. Needed before step F.**
- **Recommend rendering in place:** remove Routes.razor's inner LayoutView. The two addresses stay, and A.6.3's Route is unchanged.
- **Recommend not naming the page or its admitting roles in the pilot.** That needs PageAccess promoted from the test project and a route-to-label map. S1's copy needs neither.

**D7. The error bar's Dismiss and reference.**
- **Recommend keeping Dismiss,** with the copy "This page no longer responds; copy anything you need, then reload."
- **Recommend no per-circuit reference in the pilot.** It is new code; file it as a follow-up.

**D8. No-role heads and claim holders. Needed before step F.**
- **Recommend:** with no role, show no "Acting as" head.
- My Progress for a trainee_record claim holder is a personal link beside My Data Rights, shown whatever the acting role (the graduate Assessor case).

**D9. Where the phone menu puts the account row. Needed before step F.**
- R2 puts the name and Sign out above the links. With one Sign out form in the DOM and a CSS-only toggle, that position cannot be built cleanly.
- **Recommend the foot of the panel (A's order),** pinned so it stays in view under the Administrator's scrolling list.

**D10. T190 page titles. Needed before step F.**
- Either apply "<Page> · Wombat" in sentence case to every page, or to none. Applying it to some pages leaves the titles less consistent than today.
- **Recommend every page,** in one mechanical commit with a reflection test in AppHeadTests (re-check steps 1.3, 1.25 and 1.26).

**D11. The Source Sans 3 licence and the dark-mode record. Needed before step F.**
- The flow README says body text, dark mode and reduced motion were settled in the round-2 message, but the answers are not recorded anywhere.
- OFL is a font licence, not a GPL-compatible software licence. Fraunces already relies on the argument that a separately served font file is aggregation.
- **Recommend:**
  - Record a W-decision accepting OFL fonts on the aggregation basis.
  - Vendor Adobe's unmodified release woff2 files. The "Source" name may be reserved; this needs confirming against upstream LICENSE.md.
  - Add OFL.txt for Source Sans 3, and for Fraunces, which lacks one today.
  - Record dark mode as "later".
  - Make three cheap preparations now: `color-scheme: light` on `:root`, `--on-fill` (#fff) for text on filled buttons, and the shadow tokens (which S15 needs anyway).

### Nits

- **N1. Contrast table.** Add the missing pairs:
  - the page ring and input border on header-bg and the four tints;
  - the link on the tints;
  - muted text on header-bg (4.57) and on success-bg (4.52);
  - the status dots, the progress fill and the reconnect bar;
  - the phone bar.
  - ContrastTests samples the gradient rather than only its two ends.
- **N2.** Three figures are slightly off (17.52, 10.62 and 11.09 are the true values). DESIGN.md should state the method: blend a translucent colour over its real ground, then measure.
- **N3. Radii, notation and hover.**
  - Name the radii by scale (`--radius-sm/md/lg/xl/pill`).
  - Write `:root` in one notation.
  - The invalid stripe stays on `var(--danger-color)`, not a new token.
  - `--hover-bg` equals `--background-color`, so row hover shows only on a surface.
- **N4.** In Shell-Assessor, only the first row's Open action carries a per-row name. Name every row's action.
- **N5. Small frame inconsistencies.**
  - The switch alert says "one click away in the sidebar", which is wrong at 390 px. Shorten it to "You are now acting as Assessor."
  - Use one role-head layout that wraps.
  - The Administrator's subtitle should follow "<role> · <period>".
- **N6.** Each failure board should list the runbook Expects and DESIGN.md lines its wording changes ("Back to home" becomes "Go to Home", "Request ID" becomes "Reference", the h1 differs from the tab title, and the error bar no longer copies ClinicAssist). It should also name its components (new: PageHeader with an icon, the reference block).
- **N7.** Build on a T335 worktree lane in small commits, squashed to one T335 commit on master. That squash is what the README's "commit once" means.

---

## Paste-ready message for the canvas

This assumes the recommended answer to every decide. If the operator picks otherwise:
- lines 1 and 2 follow D1 and D2;
- line 5 follows D6;
- line 9 follows D8;
- line 10 follows D9;
- line 14 follows D10;
- line 21 follows D7;
- line 23 follows D4;
- line 27 follows D11.

```
Round 2 is accepted with changes. Please revise the same boards (no new variations) with these fixes; keep everything else.

Switching and navigation
1. Write the rule on the canvas: access is the union of held roles and never changes. In-app navigation never switches role; a page outside the acting role's nav keeps the role. Only a link that names a role (an email's) switches, and only to a role the person holds, via /dashboard/switch/{role}?returnUrl=<local path>. "Switch back" returns to the same page. Redraw R2-Detail-Email to match.
2. State: the acting role is remembered per account across sign-ins; the next person on the same browser lands by the normal precedence.
3. Keep the brand cell at the head of the sidebar, as in R2.
4. R2-Landing-Assessor alert: "You are now acting as Assessor." (drop "one click away in the sidebar").
5. R2-Denied-In: remove the "Switch to Assessor and open" panel (it cannot occur). Refusal copy: "None of your roles (Committee member, Assessor) opens this page."; one role: "Your role (Trainee) does not open this page." Don't name the page or the roles that admit it. Ask line: "ask your institution's Wombat administrator"; for an Institutional admin, "ask the platform administrator".
6. Add an active-item owner table: a list lights itself (aria-current=page); a page under it lights its owner for the acting role (aria-current=true). Include /admin/colleges/{id}/specialities (College admin → Specialities; Administrator → Colleges) and /msf/reports/{id}, /msf/coverage (→ MSF Campaigns). My Account, Change password and the failure pages light nothing; on My Account the top-bar name carries aria-current=page. Retire "lights Home". Breadcrumbs follow the owning list.
7. 1280 sidebar: draw the Administrator's list scrolling under a fixed brand cell and role head at a ~700 px viewport, with the first and last items focused; 2–4 px between rows so the ring neither spills nor clips; the list takes no tabindex.
8. 390: add an open board for Thandi Zulu with the switch under the role head (44 px); draw the folded bar with "Sub-speciality admin"; state the Pending trainee head.
9. No role (a former trainee): no "Acting as" head. My Progress for a trainee record is a personal link beside My data rights, shown whatever the acting role (also for a graduate acting as Assessor). Draw the "Change role" disclosure for three or more roles once.
10. Phone menu: name and Sign out at the panel's foot, pinned so they stay in view under the Administrator's scrolling list.
11. Menu toggle: no aria-expanded on the label; draw it focused (white ring on the label, driven by the checkbox's :focus-visible).

Content
12. Names as stored: first and last name, no "Dr" ("Thandi Zulu", "Mohammed Patel", "Anele Dlamini"). The Administrator is "Demo Administrator"; show a no-name account as its truncated email. Use cast rows (Nomsa Mahlangu's Mini-CEX, not "Thandi Nkosi").
13. Every row action named per row ("Open Mini-CEX for Nomsa Mahlangu"), not only the first.
14. Sentence case everywhere; nav label, h1, breadcrumb and title stem are the same words: "Activity inbox · Wombat", "Log an activity", "My data rights".
15. Landing cards belong to later flows: label them placeholders and drop the duplicates. Assessor: one "Waiting for your rating" card with the count as its badge, no "Pending requests". Administrator: job status in one card only; drop Maintenance, but keep /admin/curriculum-progress reachable. Administrator subtitle follows "<role> · <period>".

Failure pages
16. Access denied, signed out (only reached by typing it): with a local ReturnUrl, Sign in → /account/login?ReturnUrl=<it> and "brings you back to the page you asked for"; without one, "Sign in to carry on." Tab title matches the h1.
17. Not found: delete "A record from another institution is reported this way too." Drop the echoed address (the address bar keeps it). Signed out: the only entrance is typing /not-found, so no address and no "sign in first" line.
18. Error page: Reference is the 32-character trace id, not the 55-character traceparent. Annotation: "static, rendered once inside the failed request"; Try again goes to the failed address. Add the typed-/Error state: nothing went wrong, no reference, no Try again, Go to Home. Signed out: "If it keeps happening, send this reference to whoever sent you the link, or to your Wombat administrator."
19. Reconnect: drop "Try now"; when an attempt starts, the line reads "Trying again now". Failed and Resume failed: one "Try again" button; copy "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost." Paused: "This page is paused. Resume to carry on." Add the rejected state: "Reloading the page…", no button. 390 width: 358 px (16 px each side).
20. Error bar: the flex row sits on an inner wrapper (the runtime sets display:block on #blazor-error-ui); the buttons keep .reload and .dismiss. Add a 390 variant: the sentence, then Reload and Dismiss side by side at 44 px.
21. Dismiss copy: "This page no longer responds; copy anything you need, then reload." No reference on the bar.
22. On each failure board, list the runbook wording it changes ("Back to home" → "Go to Home", "Request ID" → "Reference", h1 vs tab title) and name the new components (PageHeader with icon, reference block).

Token sheet
23. nav-active-bg: show the swatch as #62779f (the blend at the gradient start), note that it is translucent and valid only over the gradient; alpha .32, with the 3 px white bar and weight 600 as the recorded state cue. Add --nav-text-strong (#fff) for the current and hovered item and the role name; mark nav-text on nav-active-bg (3.14) as forbidden.
24. Every focusable control on dark chrome uses --nav-focus-ring (brand link, Menu toggle, nav links, switch, account link, Sign out; 1280 and 390; the signed-out gradient bar). Add the phone-bar and signed-out-bar pairs.
25. Tokenise what the boards use: --link-hover #084a8a, --nav-divider rgb(255 255 255 / .18), shadows (keep --shadow-color; add raised, dialog, bar). The mark is /brand/wombat-mark.svg at 32 px. Mark #5b6068, #e6e8eb, the .slot stripes and the scroll-fade as canvas-only.
26. .btn-outline has background var(--surface-color); say so (transparent fails on header-bg and success-bg).
27. Add color-scheme: light and --on-fill (#fff) for text on filled buttons (dark mode is later, not now).
28. Motion: --motion-fast 150 ms; under prefers-reduced-motion the skeleton is a still header-bg block, the reconnect spinner, bar, slide and fade stop, and transitions go to 0.
29. Contrast table: add the page ring and input border on header-bg and the tints, the link on the tints, muted text on header-bg (4.57) and success-bg (4.52), the status dots, the progress fill, the reconnect bar and the phone bar. State the method (blend translucent colours over the real ground; sample the gradient).
30. Radii by scale (--radius-sm/-md/-lg/-xl/-pill). The invalid stripe stays inset 4px 0 0 var(--danger-color), not a new token.
```

---

## Step F build plan (condensed)

**0. Before starting**
- The operator answers D1, D2, D6, D8, D9, D10 and D11. Round 3 comes back from the canvas.
- Commit the design record (`design/flows/01-shell/`, untracked today).
- Update the README's step F Run section to all five suites.
- Work in a T335 worktree lane with small commits, squashed to one T335 commit on master.

**Commits, in order.** Each commit carries its own DESIGN.md section and its tests.

1. **Tokens and type.**
   - `:root`:
     - changed: success, danger, warning, input-border, focus-ring;
     - new: the nav-* tokens including nav-text-strong and nav-divider, scrim, link-hover, shadows, motion, radii by scale, on-fill;
     - removed: accent and info;
     - `color-scheme: light`.
   - Fonts:
     - Source Sans 3 woff2 files and OFL.txt for both faces in wwwroot/fonts, loaded by an `@font-face` with swap;
     - `--font-body` and `--font-mono`, line-heights, `font: inherit` on controls, tabular-nums on tables.
   - Components: alerts with text on the tint, badges, buttons (`.btn-outline` with the surface fill), validation, `:focus-visible`, reduced motion.
   - Raw colours leave: lightyellow becomes `--warning-bg`, and the backdrop literal becomes `--scrim`.
   - Then re-check A.7.2 to A.7.12 at 390 px before going on.
2. **Role labels and the display name.** A role-label map in the Domain beside WombatRoles (reuse NomineeGate's). The display-name claim in WombatUserClaimsPrincipalFactory, falling back to the email.
3. **The acting role.**
   - Resolved in App.razor, passed as a `<Routes>` parameter and cascaded.
   - `/dashboard/switch/{role}`:
     - the cookie is bound to the account (D1);
     - `?returnUrl=` is local only (D2);
     - a successful switch sets a one-time flag;
     - an unheld role is refused and sets no flag;
     - sign-out deletes the cookie.
   - Home reads the cascade.
4. **Nav content.**
   - Grouped per acting role; headings are not links; no Logout or My Account; no stubs; the personal claim link (D8).
   - Delete PlaceholderPage, with the runbook Route lines and coverage.md rows in the same commit.
   - The DESIGN.md nav table, and its parser in the tests.
5. **Shell layout and phone.**
   - MainLayout and NavMenu markup and CSS.
   - The checkbox first, keyed by path; the ring on the label; the account row at the panel's foot; one Sign out form.
   - The sidebar as a flex column with a scrolling list (no tabindex, padding-block) and a full-height gradient column.
   - The signed-out shell is the top bar only.
   - The shell reads no data.
6. **The active item.** `NavOwners` (page type and acting role to owner item), the RouteData cascade, and `NavItemLink`.
7. **Breadcrumbs** through PageHeader; below 641 px they fold to a single parent link.
8. **Home and the dashboards' frame.**
   - h1 "Home"; the subtitle "<role> · Semester N, YYYY" from a pure function of the date; a header action per role.
   - The switch alert takes focus, and it must win over FocusOnNavigate.
   - DashboardCard IsLoading; one error Alert with Try again; null guards.
   - The grid track fix; today's cards are kept.
9. **The failure pages and T321.**
   - Routes loses its inner LayoutView; Access denied renders in place (D6) with claims-only copy.
   - Not found: new copy, no echo.
   - Error.razor: static and anonymous, behind UseExceptionHandler. It shows the trace id (and logs it with the exception), the time in SAST, Try again from the exception feature, and the typed state.
10. **The reconnect dialog and the error bar (T330).**
    - Reconnect: one state shown at a time; `retry()` and `resume()` under Try again; no Try now; the new copy and the rejected line; a live region updated only when the state changes; focus on show.
    - Error bar: the inner wrapper and the 390 px layout.
11. **T190 titles (D10):** one mechanical commit with a reflection test.
12. **Docs.**
    - The DESIGN.md banner and leftovers, the runbook Expects, coverage.md, states.md, BRIEF § 5.1's component list.
    - W-decisions for the font licence, the GET switch and the switch's memory.

Then run `dotnet build Wombat.sln -c Release` and all five suites (Domain, Infrastructure, Integration, Web, Architecture), without `--no-build`, and quote the counts.

**DESIGN.md sections to amend**
- The banner: "Redesigned so far: flow 01", with the date and the commit.
- Files that own the design system: the new components and wwwroot/fonts.
- Design tokens: the new block, the contrast table and its method, the nav-* tokens, and fixes to the stale values.
- Logo and brand: the 56 px cell and 32 px mark.
- Typography.
- Layout grid: R2's shell, the phone bar, the error bar and the gradient column.
- The NavMenu, rewritten:
  - nav per acting role, grouped;
  - the owner rule;
  - the role head and the switch;
  - the claim link;
  - no stubs, and the rule that the nav never links to an unbuilt page;
  - the signed-out shell.
- Button system: the header height and `.btn-outline`'s fill.
- Dashboard layout grid: three tracks.
- Alerts, validation and empty states: text on the tint, and the load-error rule.
- Skeleton loaders: reduced motion.
- Badges and status dots.
- Accessibility: the nav ring, one active item, reduced motion, target sizes, the scroll region.
- Page-level patterns:
  - breadcrumbs;
  - the Dashboard page (drop "Welcome/Viewing as");
  - a new System pages subsection.
- app.css section order.
- Non-negotiables: the `.reload` and `.dismiss` allowance, and the reconnect template script's exemption from the 10-line rule.

**Tests**
- **To change:**
  - NavMenuAuthorizationTests (the parser, per acting role, the graduate cases, placeholder becomes "no /placeholder/ link");
  - DashboardLinkAuthorizationTests (a cascading acting role, not IHttpContextAccessor);
  - PageAccess;
  - DashboardPriority, DashboardCard, WaitingCards and AssessorDashboardSubjectName;
  - NarrowLayout, DesignSystemSmoke, PageShapeSmoke, DefinedClass, FieldGroup, InvalidFieldStyle, BadgeForStatusTable and AlertRole;
  - ActionFocusTests and AccessibleNamesTests;
  - BlazorEndpointAccessTests, AppHeadTests (if D10) and AppAssetUrlTests (if a script is added);
  - SessionEndTests; Scenario via the runbook edits.
- **To add:**
  - ContrastTests: the sampled gradient, the forbidden pairs, `.btn-outline`;
  - ActiveNavItemTests: at most one active item per route per acting role, the T331 cases, failure pages light nothing, aria-current rules;
  - RoleSwitchTests: the nav changes with the switch, only held roles are offered, the three-or-more disclosure, a forged switch shows no alert;
  - MainLayoutTests: one Sign out, the name claim with its fallback, signed out shows no nav;
  - HomeFrameTests (T329);
  - Routes: Access denied renders once;
  - ReconnectModalTests (T330);
  - Integration ActingRoleFlowTests (T317): another account ignores the cookie, sign-out deletes it, returnUrl is local only;
  - Integration ErrorPageFlowTests (T321): a 500 with the page, the CSP nonce, the id logged, signed out, and no page for a POST or a non-HTML request.

**Risks**
- **Render modes:**
  - the acting role must reach the circuit as a parameter;
  - /Error must stay excluded from interactive routing;
  - the NavMenu must re-render on in-circuit navigation;
  - the switch alert's focus competes with FocusOnNavigate(h1);
  - a switch must start a fresh circuit, from both a static page and an interactive one (check in the browser).
- **The static shell:** /Error, /portfolio/verify and /msf/respond render the shell with no circuit, so no `@onclick` or `@bind`. The shell reads no database, or an outage takes /Error down too.
- **The toggle:** the key per path, the ring on the label, the account row's pinned foot.
- **The sidebar's height** at 800 px, and the ring clipping.
- **The global reflow** from the font and line-height change.
- **UseExceptionHandler's position** relative to SecurityHeadersMiddleware and UseWombatNotFoundPage.
- **Semantics:** the cookie now drives the nav, the landing, the active item and the breadcrumbs, so Step 2.34 changes meaning.
- **Font licence:** whether upstream reserves the "Source" name needs confirming.
- **Size:** about 60 to 70 files and 3,500 to 4,500 lines, an XL task: 4 to 6 sessions for step F and 1 to 2 for the replay.

**Acceptance (step G)**
- BRIEF § 9 on a fresh database: the whole runbook, Acts 1 to 6 and then the appendix.
- Every Expect holds: flow 01's 21 steps and every step that quotes the frame. The README's search found 65 steps and Build's 81; check the union. About 40 to 45 Expects need rewording in the same task.
- Nav labels: 1.1, 1.2, 1.8, 1.10, 1.12, 2.1, 2.13, 2.19, 2.28, 2.32, 2.33, 2.36 to 2.39, 2.42, 3.1, 4.1, 4.2, 4.4 to 4.6, 4.9, 4.10, 4.26, 5.21, 6.10, 6.14, 6.20, A.1.1, A.1.3, A.2.1, A.3.1, A.5.2, A.6.3, A.7.14.
- "Welcome", "Viewing as" or the switch line: 1.1, 1.8, 1.10, 2.8 to 2.10, 2.18, 2.27, 2.31, 2.33, 2.34, 2.36 to 2.38, 2.44, 3.33, 3.54, 4.3, A.4.7, A.6.8.
- The switch: 2.34, 3.33, 3.52, A.5.3.
- Sign out: 2.8 to 2.10, 2.18, 2.19, 2.27, 2.34, 5.13, 5.21, A.4.7, A.5.4, A.5.8; on the phone, A.7.2 and A.7.4.
- Placeholders: 3.31, 3.51 to 3.54, A.5.4, A.5.9 to A.5.13, A.7.7, A.7.8.
- Failure pages: 1.15, 1.23, 2.19, 2.32, 4.5, 4.41, 5.22, 6.10, A.1.4, A.5.1 to A.5.8, A.6.3.
- The signed-out static shell: 5.13 and 5.14.
- The phone: A.7.2 to A.7.12. Contrast and axe: A.7.14. Titles, if D10: 1.3, 1.25, 1.26.
- Re-capture the baseline states in flow 01 § 3 and § 4.
- Check the reconnect states by suspending the app (states.md § Shell and framework).
- Force a server exception to check the error page, its reference in the log, and Try again.