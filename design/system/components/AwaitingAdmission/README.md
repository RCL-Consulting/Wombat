# AwaitingAdmission

The pending registrar's Home (`/`, the Pending trainee's dashboard, `Pages/Dashboards/TraineeDashboard.razor`, its `Pending` branch; read in Home's one read as `PendingAdmissionDto`): **one card, "Awaiting admission"**, that says where the registrar stands, who invited and who admits, what admission will change, and what can be done meanwhile. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-P01 to P09; round 3 F3, F8, M2, M7, S5, check rules 4 and 7) drew it. Until then the card had the `info` icon and the emphasis stripe, and said nothing about the programme, the institution or who admits.

## Who reads it

A Pending trainee: a registrar who registered from a Trainee invitation, or was provisioned by an institution's sign-in, and is not yet admitted. Her menu is Home and My data rights (D55: she files nothing). Home's header is "Home" with "Pending trainee · Semester 2, 2026" and no action.

## The card

A `DashboardCard` "Awaiting admission" (`section.detail-card.dashboard-span-3.wait-card`, `#card-awaiting`), its title `h2.dashboard-card-title` with **`hourglass`** (18px): **spanning three, no stripe, the control edge** (F8; `.wait-card`: `border-color: var(--input-border)`, 3.80:1, where a card's hairline is `border-color`): the page's one subject, and nothing for her to act on. Its body is a flex column, its parts 16px apart, none with a margin of its own.

1. **The lead**: "You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you."; with no invited speciality, "… waiting to be admitted at Kgosi Kgari Teaching Hospital. …" (check rule 4: no discipline named).
2. **A details list** (`dl.detail-list`):
   - **Institution**: "Kgosi Kgari Teaching Hospital".
   - **Programme**: "Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1)": the invited disciplines and the curriculum adopted for her invited sub-speciality (M2); the disciplines alone when none is adopted; **no line** with no invited scope (S5: an SSO-provisioned registrar).
   - **Invited by**: "Nolwazi Mbatha, Institutional admin, on 2026-10-20", the day the invitation was issued on the South African calendar (after a resend, the current link's). **Shown only when the invitation was issued as an Institutional admin** (`Invitation.IssuedAsRole`, F3) **and the issuer, read now, is neither deactivated nor erased** (check rule 7).
   - **Who admits you**: "An Institutional admin at Kgosi Kgari Teaching Hospital.", with " If it is taking long, ask Nolwazi Mbatha." only beside an Invited by line.
3. **"When you are admitted"** (`h3`, 1rem in the card), then `ol.account-steps`: "Your programme start date is set, and with it your training year.", "You are signed out once, and sign in again as a Trainee.", "Then, once your programme has started, you can log activities and follow your progress.", "An email tells you, at mahlangu@kgk.wombat.local, that it has happened." (her address with `<wbr>` after its @, M7).
4. **"Meanwhile"** (`h3`): "You can check your account and your data rights.", then `div.wait-links`: **Review your account** (My account, `/account/profile`) and **Open My data rights** (`/account/data-rights`), each a link with `arrow-right` after its words (`.wait-links a`: inline-flex, 4px gap; the links 8px by 24px apart, wrapping).

## States

- **Loading**: the card's title over DashboardCard's skeleton, as every Home card (DashboardFrame).
- **Load failed**: Home's one read, so DashboardFrame's "**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes." with Try again (P09).

## Narrow widths

Below 641px the two links are 44px targets (`.wait-card .wait-links a`). The details list goes to one column, each term over its value, 4px apart (`.detail-list` below 641px). Nothing scrolls sideways at 390.

## Contrast

No new colour token. The card's edge, `input-border`, 3.80:1 on the surface and 3.60:1 on the page's `background-color`; its words 12.63:1 and the terms `muted-text` 5.09:1 on the surface; the links `link-color` 6.70:1.

## Known gaps

- **An Administrator who issues an invitation, whatever role they act in, is recorded as Administrator** (`IssuedAsRole` is the role the issue was authorised under, build contracts § 1): their invitee's card shows no "Invited by" line and no "ask" (`states/home--awaiting-admission-admin-issued.png`).
