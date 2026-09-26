---
id: T332
title: No design brief exists for the Claude Design GUI redesign: the journeys, states and baseline are ready but nothing turns them into briefs
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T332 — No design brief exists for the Claude Design GUI redesign: the journeys, states and baseline are ready but nothing turns them into briefs

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The operator is about to redesign the GUI with Claude Design. It builds a design system from code,
but the user brings the journeys, one flow at a time. Without a brief it will "pick one of its favorite aesthetics"
and produce designs that cannot be built in Blazor under Wombat's CSP and design contract.
**Surfaced:** 2026-09-26, after T293–T295 made the journey catalogue and the screenshot baseline.

## Symptom

`coverage.md` (every role's jobs), `states.md` (577 states) and `design/baseline/` (1,250 screenshots) exist, but no
document tells Claude Design what Wombat is, what it may and may not change, what to link, which flows to design in what
order, or what the redesign must fix (the presentation debt filed as T299, T306, T308, T311, T314, T316, T321–T328,
T330, T331).

## What to build

`design/` beside the gitignored baseline:
- `design/BRIEF.md`: the master brief. It covers the product and its people; what to link and upload (only
  `src/Wombat.Web/wwwroot`, never the working tree, which holds `pwd_DO_NOT_COMMIT.txt` and `recovery/`); the
  technical constraints a design must meet to be buildable (Blazor Server, static SSR pages for signed-out visitors,
  tokens only, Lucide icons, self-hosted fonts, CSP, no CDN or Tailwind); which of `DESIGN.md`'s rules are technical and
  which are policy open to change; the reskin-or-restructure decision with a recommendation; the requirements the
  defect backlog sets; the screens that do not exist yet; the flow order; and the handoff back to Claude Code with its
  acceptance check (replay the flow's runbook steps).
- `design/flows/<nn>-<flow>.md`: one brief per flow, ready to paste. Each gives the goal, the people, the steps and
  pages (runbook step ids), the states to show, the screenshots to attach, the known defects, and the questions the
  design must answer.

## Verification

- [x] Every claim about Claude Design cites the verified research (2026-09-26); every claim about Wombat cites a file,
  a task or a runbook step — an adversarial review checked each Claude Design claim against the research's verdicts
  (four overstated claims corrected), resolved all 215 file:line citations, and spot-checked a claim per flow (one
  wrong, fixed). The research is committed as `design/research/claude-design-2026-09-26.json`.
- [x] Every group-3 task (the presentation debt) appears as a requirement or a missing screen — all 16 are in
  `BRIEF.md` § 6 or § 7, and the five "Coming soon" pages are § 7 B7.
- [x] Every role in `coverage.md` § Journeys by role is served by at least one flow brief — all 12 role headings.
- [x] Every screenshot path a flow brief names exists under `design/baseline/` — `design/tools/check_baseline_paths.py`:
  every attachment exists; the two names it reports are captures the brief says were never taken
  (`change-password--throttled`, the December notice). `check_flow_completeness.py`: all 324 steps and 80 templates are in
  a flow. `check_verbatim_steps.py`: 384 quoted steps match the runbook.

## As built — 2026-09-26

- `design/BRIEF.md` (the master brief, about 550 lines) and 18 flow briefs, `design/flows/01-shell.md` to
  `18-platform-operations.md`, ordered shell and highest-frequency journeys first. Each flow brief carries a paste-ready
  ask (goal, audience, screens, steps, states, requirements, questions, attachments and the constraints digest), the
  journey with its runbook step ids, the states to design, the screenshots to attach, the tasks it must solve, and its
  acceptance replay.
- **The recommendation (§ 4):** restructure the frame (shell, navigation, role dashboards and the screens missing from
  them) and reskin the page shapes. The brief says what either choice costs in `DESIGN.md` and its tests. The operator
  decides.
- **Where to run it (§ 2.2):** `/design` in a Claude Code session in this repo. That is inference: origin is not on
  GitHub, so claude.ai/design cannot link it, and only tracked files may be uploaded (§ 3, with a `git ls-files`
  staging snippet; never the working tree, which holds `pwd_DO_NOT_COMMIT.txt` and `recovery/`).
- **The baseline (§ 10):** the pages group 1 changes (T297, T300, T302, T303, T307) are marked pre-fix, and are
  refreshed as each fix lands. Invitation captures taken just after Issue or Resend show a registration link: crop it
  or leave the image out.
- **Seen while writing, noted rather than filed:** Home greets by email (T324), and an undefined `--text-muted` token,
  underlined nav items and a short sidebar gradient (T328).
- **For the operator to confirm in Claude Design:** whether it accepts a `.md` upload; whether `/design` reads the
  project's design system as the artifact skill does; whether an imported design system is a frozen snapshot.

## Related

T293, T295 (the catalogue and the baseline), T297–T331 (the findings), `execution/architecture/DESIGN.md`.
