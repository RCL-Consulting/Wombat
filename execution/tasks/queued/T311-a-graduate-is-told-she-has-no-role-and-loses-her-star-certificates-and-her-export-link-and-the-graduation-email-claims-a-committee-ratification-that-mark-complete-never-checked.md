---
id: T311
title: A graduate is told she has no role and loses her STAR certificates and her export link, and the graduation email claims a committee ratification that Mark complete never checked
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T311 — A graduate is told she has no role and loses her STAR certificates and her export link, and the graduation email claims a committee ratification that Mark complete never checked

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A graduate cannot download her own STAR certificates. Each certificate is the one document per EPA that records what she is entrusted with. Home tells her to ask an administrator for a role. On its own, the email's false sentence would be Low. It is here because the same email promises the portfolio that this task makes reachable.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-5.21a, F-A.7.4a, F-5.22a, F-5.23a).

## Symptom

After Prof Mbatha marks Dr Molefe's programme complete (runbook Step 5.17):
- Home (Steps 5.21 and A.7.4) reads 'Welcome, molefe@kgk.wombat.local / No role assigned / Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.' It does not say her programme ended and does not point to My progress (design/baseline/act-5/5.21-1-molefe-home-graduate.png, design/baseline/states/home--no-role.png, home--narrow-former-trainee.png). T252's As built says 'Home says the same', but it was checked only on a withdrawn trainee, who keeps the role (home--trainee-ended.png).
- My authorisations (Step 5.22): /portfolio/authorisations redirects to /access-denied (5.22-2-molefe-authorisations-denied.png). It is the only page that downloads a STAR certificate, and Step 5.8 downloads one before completion for that reason.
- Export Portfolio (Step 5.23): her menu is Home, My Account, Data Rights, My Progress and Logout. Typing /portfolio/export admits her and produces a PDF that verifies (5.23-1-graduate-exported.png), but nothing links to it.
- The graduation email (Step 5.18) says 'Your committee has ratified your final entrustment decisions and your portfolio of evidence is available in Wombat.' It sends the same words whatever the committee did, and the portfolio is reachable only by typing its address.

## Root cause

- CompleteTraineeProfile.cs:93-94 removes the Trainee role ('there is no Alumnus role'). DeactivateTraineeProfile does not, so a withdrawn trainee keeps TraineeDashboard, whose Curriculum targets card carries T252's ended line (TraineeDashboard.razor:31-35, [Authorize(Roles = "Trainee,PendingTrainee")] at :3).
- Home.razor:29-71 switches on roles only. A graduate resolves to no role and falls to the default branch (:61-70), which was written for 'an administrator removed the last one'. The trainee_record claim (WombatUserClaimsPrincipalFactory.cs:56) is never read there.
- MyAuthorisations.razor:2 is [Authorize(Roles = "Trainee")]. Its reads, GetActiveDecisionsForTraineeQuery and DownloadEntrustmentCertificateCommand (:73), already admit the trainee herself through TraineeScopeResolver.MayReadAsync, so only the page attribute refuses her.
- NavMenu.razor:146 gives the claim-keyed section only [MyProgress]. NavMenuAuthorizationTests.cs:150-160 pins that, with the reason 'the role's work, filing and exporting included, is not offered'. No decision records why exporting her own record is the role's work. ExportPortfolio.razor:3 is a bare [Authorize], and ExportPortfolioCommandHandler admits her as herself.
- DESIGN.md:236 names MSF Reports and My Committee Reviews as the graduate's pages that still require the role. My authorisations is named nowhere.
- GraduationEmail.cs:15-16 and :24-25 are fixed text. T166's As built: 'Recording a Graduate decision or completing a programme checks nothing' (the gating is deferred, and EPA-PROGRAMME § 4 lists it as an operator decision).

## What to build

1. **Home for a former trainee.** A user who resolves to no role but holds the trainee_record claim gets a former-trainee card, not 'No role assigned'. It is its own dashboard component, keyed on the claim. The card holds:
   - the ended line T252 decided: QuotaText.ProgrammeEnded plus ', so no target applies to you any more.';
   - links to My progress, My authorisations and Export Portfolio.
   'No role assigned' stays for an account with neither a role nor the claim. A graduate who later holds a role (for example Assessor) keeps that role's dashboard, as now.
2. **My authorisations admits the claim:** [Authorize(Policy = "TraineeOrFormerTrainee")]. The page stays as it is today, and the Download buttons work for her own decisions.
3. **The NavMenu's trainee_record section** offers My Progress and Export Portfolio. Update NavMenuAuthorizationTests' graduate case and its reason. This reverses T252's 'not offered' on purpose: the page admits her and the email promises the portfolio.
4. **The graduation email says only what is true:**
   - the programme was marked complete on that day;
   - her record stays in Wombat (My progress, her STAR certificates and the portfolio export), with a link to /portfolio/progress, as AssessmentCompletedEmail links to its page.
   Drop 'Your committee has ratified your final entrustment decisions'. Once T166's gating is decided, the email may name the ratified Graduate decision when one exists.
5. **Docs.** DESIGN § The NavMenu (:229-236) and § Dashboard page (:1811) say what a graduate is offered.

Not in scope:
- MSF Reports and My Committee Reviews. DESIGN keeps them role-only; change them only by decision.
- /activities/new admitting a graduate (F-5.22b).
- Whether Graduate may be recorded at an annual review. T082 makes the two progression types one decision to the engine, so decide it with T166's deferred gating.

## Verification

- [ ] bUnit: Home rendered for a principal with no role and the trainee_record claim shows the ended line and links to /portfolio/progress, /portfolio/authorisations and /portfolio/export, and no 'No role assigned'. With neither a role nor the claim, it still shows 'No role assigned'.
- [ ] DashboardLinkAuthorizationTests: every link on the former-trainee card opens a page whose policy admits a claim-only holder.
- [ ] NavMenuAuthorizationTests: a claim-only holder is offered My Progress and Export Portfolio and is admitted by /portfolio/progress, /portfolio/export and /portfolio/authorisations. A PendingTrainee without the claim is offered and admitted to none of them.
- [ ] Application test: for a completed trainee holding no role, GetActiveDecisionsForTraineeQuery returns her decisions and DownloadEntrustmentCertificateCommand returns a certificate.
- [ ] Unit test on GraduationEmail.Build: no 'ratified' sentence in either body, and both link to My progress.
- [ ] Browser, replay Steps 5.17-5.23 and A.7.4 on a fresh scenario database. Home shows 'You completed your programme on …' with the three links. /portfolio/authorisations lists her 15 STARs and downloads one. Export Portfolio is in her menu. The Step 5.18 email reads the new text. Refresh the 5.21 and 5.22 screenshots and give the runbook owner the new Expects.

## Related

T252 (the claim, the TraineeOrFormerTrainee policy, 'Home says the same'), T080/T081 (graduation lifecycle), T166 (gating deferred; graduation behaviour is an operator decision), T082 (review types), T141 (the Trainee-only nav section), T261 (Home's per-role switch), DESIGN § The NavMenu and § Dashboard page. Runbook Steps 5.8, 5.17-5.23 and A.7.4. Findings F-5.21a, F-A.7.4a, F-5.22a and F-5.23a. F-5.22b (/activities/new) is not this task.
