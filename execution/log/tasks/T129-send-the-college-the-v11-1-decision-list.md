---
id: T129
title: Send the College the v11.1 decision list
status: done
priority: P1
owner: operator
depends_on: []
created: 2026-09-20
completed: 2026-09-20
---

# T129 — Fourteen product decisions are being taken by default, because nobody has asked the College

**Status:** done 2026-09-20 — all fourteen answered and transcribed.
**Severity:** High — it is the critical path for Waves 3 and 4 of the EPA programme
([`knowledge/EPA-PROGRAMME.md`](../../knowledge/EPA-PROGRAMME.md) § 4). Nothing about it is
technically hard; it has simply never been sent.
**Surfaced:** 2026-09-20, asking whether the EPA work is actually first in the queue. It is
Wave 1 item 1 and has been since 2026-09-19, but it existed only as a line in a planning
document — never as a task, and so never as something the queue could surface.

## Symptom

Nine of the open EPA tasks are marked **NEEDS A DECISION** in the § 2 inventory. Every one of
them waits on decisions in § 3B, which are addressed to the College and have never been asked.

Meanwhile the decisions are being made anyway — by default, by whoever writes the next seed.
§ 3's own preamble says it plainly: *"each is a judgement, and every one of them is currently
being made by default rather than deliberately."*

This is the same shape of defect as [T128] and as T097's undeployed backup: the work was
designed, written down, and then not routed to anyone who could act on it.

## Root cause

The programme document is a planning artefact, not a queue. Wave 1 item 1 reads *"Send the
College D1, D4, D6–D16 and D37. One message."* — but the register is what the session-start
bundle renders, and the register had no row for it. Anything not filed is invisible.

## What to build

**The message is written.** [`knowledge/college-rfi-v11-1.md`](../../knowledge/college-rfi-v11-1.md)
covers all fourteen (D1, D4, D6–D16, D37), translated out of implementation language and into
terms a clinician can answer. Every question carries a proposed default so the cheapest valid
reply is *"defaults confirmed"*; four are marked as having no safe default.

Remaining work is operator work, not engineering:

1. Fill in the addressee and the reply-by date.
2. Read it once as the College will read it — it makes commitments about what Wombat will do
   if nobody replies, and those commitments should be ones you are willing to keep.
3. Send it, and record the date here.
4. On reply: transcribe each answer into § 3 of `EPA-PROGRAMME.md`, moving each decision from
   § 3B to § 3A with the answer and the date. **Do not leave the answers in an inbox** — the
   register is what the next session reads.
5. Re-rate the tasks each answer releases, and unblock [T130].

## Verification

Sent, answered in two passes, and transcribed. Completed 2026-09-20.

- [x] Message written and sent — `knowledge/college-rfi-v11-1.md`, answers written inline by the
      College. Eight came back first; the remaining six after D6, D7, D8, D12, D13 and D15 were put
      back with the reason each was not yet an answer
- [x] **All fourteen transcribed** into `EPA-PROGRAMME.md` § 3A-ii, each with the option chosen and
      its consequence — checked by `grep -c "CLOSED 2026-09-20"` returning 14
- [x] Tasks released re-rated — [T130] unblocked (`depends_on` emptied), [T121]'s four blocking
      decisions all answered, [T120]'s stated critical path discharged
- [x] [T121]'s local `D1`–`D4` renumbered to the College's **D9, D8, D10, D11**. They collided head-on
      with the product register — the exact trap `DECISIONS.md` splits its prefixes to avoid

### Two things the reply did NOT settle

Both are one line from the College, and **neither should be guessed**:

1. **The scope of the "clinical observed interaction" merge.** D12 settled that EPA 7 does not
   exclude Direct observation, and that a Mini-CEX and a clinical examination are one instrument —
   which changes no EPA's permitted set, because EPA 7 already names both. But the reply also
   proposed absorbing *"handover, communication etc."*, and handover is page 8's definition of
   **Direct observation**, not Mini-CEX. Merging those two changes **eight** EPAs, and makes
   **EPA 10 — "Leading and operating within a clinical team", published allow-list exactly "MSF,
   Direct observation (2)" — creditable by a Mini-CEX.** That is what [T122] exists to prevent.
2. **Whether June falls in semester 1 or semester 2.** D13 gave "boundary in June". Recorded as
   Jan–Jun / Jul–Nov. A June encounter is the only thing that moves if that is wrong, and [T130]
   buckets on it.

## Related

Wave 1 item 1 of `knowledge/EPA-PROGRAMME.md` § 4. Gates [T120] (D1, D4, D6), [T121] (D8–D11,
D37), [T122] (D12), [T130] (D13, D14), and the descriptor question in D16 which, answered one
way, is the largest unplanned item in the programme.

Same failure mode as [T128] — designed, documented, unrouted.

## Notes

- **Observed:** D2, D3 and D5 are already closed by [T124]'s page-8 extraction and are not in
  the message.
- **Observed:** the tool counts in the message were re-derived from
  `tasks/done/T098-data/annexure-a.json` rather than copied from prose. Two internal documents
  disagreed on how many EPAs name "Direct observation" — § 3's D12 said seven, [T120]'s table
  said eight. **The source says eight** (EPAs 2, 4, 6, 9, 10, 11, 13, 15). D12 has been
  corrected. The letter states no count that was not re-checked against the annexure.
- **Observed:** EPA 7 names *Directly observed clinical examination* and does **not** name
  *Direct observation*; EPA 6 names both *Direct observation* and *Case note review*. This is
  what makes D12 answerable rather than abstract, and it is quoted in the message.
- **Needs confirmation:** who the right recipient actually is. The programme says "the College
  / CPSA content owner" throughout and never names a person.
