---
id: T336
title: The emails and PDFs have no design brief or baseline, though the redesign now covers them
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-27
started: 2026-09-27
completed: 2026-09-27
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

- [x] All 15 templates in `design/baseline/mail/`, all PDFs' pages in `design/baseline/pdf/` — listed against the code:
  54 mail files (each template as PNG, HTML and text, plus a resent MSF invitation, the learner's MSF invitation and
  the digest with items); 28 PDF files (Dr Molefe's 11-page portfolio, her own and the staff export alike by SHA-256;
  Dr Dlamini's 8-page data-export summary; three STAR certificates: active, superseded and revoked).
- [x] Flows 19 and 20 in the template; BRIEF § 8 updated; `design/tools/` checks pass — 0 missing paths, all 325 steps
  in a flow, 416 quoted steps and 0 problems.

## As built — 2026-09-27

- **The inventory:** all 15 templates wrap their body in `EmailTemplateBase.WrapHtml` and have a plain-text twin. Nine
  have a sender in the code; six have none (the four Assessment templates and StarDecision, T320; PasswordReset, reset
  not built). No template exists for a revoked STAR or a data-rights decision (T319). Six emails were sent in the story
  and are baselined from the replay's real mails; the rest are rendered from the published Application DLL with the
  story's data. Every one-time token is a same-length placeholder.
- **The PDFs:** from a scratch copy of the final state; each page at 110 dpi beside its PDF. Not rendered: an expired
  certificate (no STAR in the story reaches expiry) and the notices T319 and T320 plan (they do not exist yet; flow 19
  asks for them as new designs).
- **The briefs:** `design/flows/19-emails.md` and `20-pdfs.md`; BRIEF § 8 lists 20 flows. `stage_upload.ps1` puts any
  mail that may show a one-time link in `crop-first/`.

## Related

W-008, T332, T319, T320, T313 (verification), T310 (the certificate's institution).
