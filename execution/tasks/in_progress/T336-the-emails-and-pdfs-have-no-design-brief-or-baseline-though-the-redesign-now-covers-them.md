---
id: T336
title: The emails and PDFs have no design brief or baseline, though the redesign now covers them
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-27
started: 2026-09-27
---

# T336 — The emails and PDFs have no design brief or baseline, though the redesign now covers them

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. W-008 brings the 15 email templates and the PDFs (the portfolio, the STAR certificate, the
data-export summary) into the redesign. `design/BRIEF.md` covers screens only, and `design/baseline/` holds no email or
PDF.
**Surfaced:** 2026-09-27, the operator's "include mails and pdf".

## What to build

- **Baseline:** every email template rendered as its recipient sees it (HTML and text), from the replay's real mails
  where the story sent one and from the template with the story's data where it did not (T319, T320 never send some),
  into `design/baseline/mail/`; every PDF page rendered to PNG into `design/baseline/pdf/`.
- **Brief:** `design/flows/19-emails.md` and `20-pdfs.md` in the flow template: each email's trigger, recipient, purpose,
  whether it is sent today (and the task that would send it), content and constraints (email-client HTML, plain-text
  twin, links from `Wombat:BaseUrl`); each PDF's purpose, audience, sections and constraints (QuestPDF, the integrity
  footer and `/portfolio/verify`, print). `BRIEF.md` § 8 lists both.

## Verification

- [ ] All 15 templates in `design/baseline/mail/`, all PDFs' pages in `design/baseline/pdf/` — listed against the code.
- [ ] Flows 19 and 20 in the template; BRIEF § 8 updated; `design/tools/` checks pass — the scripts.

## Related

W-008, T332, T319, T320, T313 (verification), T310 (the certificate's institution).
