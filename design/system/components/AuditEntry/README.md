# AuditEntry

An audit entry (`/admin/audit/{Id:guid}`, `Pages/Admin/Audit/AuditDetail.razor`; its words `AuditWords.cs`; its trail `AuditLinks.cs` (`ReturnFrom`, `SafeLogReturn`); the read `GetAuditEntryByIdQuery`, `AuditEntryDto`): **one action in four sections, what happened, who, what it was done to, and, for the Administrator, its payload.** Flow 18 (T400, 2026-11-11, `34b5077e`; boards R3-E02 to E09, R2-E01 to E11; Q7, Q9, Q15, T4, T101, T114, T277, K8, K9, K10, M3, R2 (the region), S3, S4) rebuilt it. Until then it was one list of fields with times in local time, a user id for the person, the payload as one unbroken line that lengthened the page, a Back to log that lost the filters, Page not found for an entry out of scope, and its read's exception text.

## Who reads it

The Administrator and the Institutional admin, each in their scope (T101: an Institutional admin reads only their institution's rows). Opened by a row's Detail on the audit log, which it lights. **The trail is the way back**: Home › Audit log › Audit entry, its Audit log crumb returning to the rows the entry was opened from (`back=`, rebuilt by `AuditLinks.SafeLogReturn` so only the log's own address is followed, T277) and naming the entry, so that row's Detail takes the focus there (R3-L13). There is no Back to log.

## The page

- **The header**: h1 and tab "Audit entry"; the subtitle says it in a line, "‹action› by ‹person›, ‹YYYY-MM-DD hh:mm:ss› SAST · ‹result›" ("TransitionActivityCommand by Nomsa Mahlangu, 2026-11-10 20:21:44 SAST · Failed"; "… by No signed-in person, …" for an actor with no name).
- **The column**: `.user-page`, `aria-busy` while it reads, of `section.detail-card.user-section`s, each named by its `h2` (`aria-labelledby`), each a `dl.detail-list`:
  - **What happened**: When, to the millisecond in SAST, with "Stored as 2026-11-10 18:21:44.861 UTC" under it (`muted text-sm`, T4); Category by label ("Other" for an unknown value); Action (`<code>`); Institution by name or "No institution"; Result, the badge (Success `badge-completed`, Failed `badge-declined`) with a failure's stored words under it (a refusal's own words, or "The action failed.", K10).
  - **Who**: Name, the name read now (an erased person's pseudonym, Q15, M3), "No person: an MSF respondent answers by link, with no account." or "No signed-in person" (K8); User id (`<code>`, "None" for none: the audit pages show ids on purpose); Address, with how much was kept under it, "Cut to its first 24 bits when stored." for an IPv4 /24, "… 48 bits …" for an IPv6 /48 (K9), "None" for none.
  - **What it was done to**: Type, Id (`<code>`), and the institution and speciality by name where the subject has them; or the one line "No subject".
  - **Payload**, the Administrator's alone (the query withholds it from anyone else, R2-E01): "As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]"." (`muted text-sm`), then the payload laid out one property a line, two spaces an indent, in its stored key order (parsed and written again, never compared as text), in `pre.code-block.code-block--region` with `tabindex="0"`, `role="region"`, `aria-labelledby` its h2: **its own focusable region**, with a ceiling of 28rem of content (`max-height: 28rem` on a `content-box`, so 30rem with its padding and border) and its own scroll, so a payload of several kilobytes never lengthens the page past it. **Named once, by the h2**: its section carries no name (S4).

## States

- **Loading**: `p.loading-line[role="status"]` "Loading this audit entry." over a skeleton card.
- **Load failed**: "Could not load this audit entry. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), titled "Audit entry", which focuses the h1 once it reads (T329: never the exception's text).
- **Audit entry unavailable** (R2-E08): an unknown id, a row of another institution and a row with none, for an Institutional admin, alike, never which (CLAUDE.md § Roles): h1 and last crumb "Audit entry unavailable" over a `.detail-card--empty`, "This audit entry does not exist, or you cannot open it.", and **Go to Audit log** (primary), which returns to the rows it was opened from.

## Narrow widths

The sections stack in the page's column at every width; below 641px each `dl.detail-list` puts a value under its term, and the payload region keeps its ceiling and its own scroll (`.code-block` wraps anywhere, so it never scrolls sideways).

## Contrast

No new colour token. The stored UTC and the address's note `muted-text`, 5.09:1 on the surface; the payload `text-color` on `header-bg`, 11.36:1; the region's focus ring is the page's global `:focus-visible`.

## Known gaps

- **Not filed** (T400's replay): What it was done to with a Type and an Id (`audit-detail--subject`, the replay's state row) is reachable only by a staged row: no command stores a subject today, so every live entry reads "No subject".
