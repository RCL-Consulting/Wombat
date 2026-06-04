# T084 — Erasure approval is not atomic (Approved-but-not-erased, no retry)

**Status:** 🔴 OPEN — found 2026-06-04 in Appendix A.1.2 play-through. **MEDIUM** (compliance integrity).

## Problem (F-A1-1)

`ApproveDataRightsRequestCommandHandler`
(`Wombat.Application/Features/DataRights/Commands/ApproveDataRightsRequest.cs`) transitions the request
to `Approved` and persists the decision, and for an **Erasure** request then calls
`IErasureExecutor.ExecuteAsync(...)` (which requires `Wombat:PseudonymSalt`). If the executor throws
(e.g. salt not configured), the request is **left in `Approved`** with the decision recorded **but the
data is never erased** — and the admin UI then shows **no further action** (Approve/Reject render only
for `Submitted`/`UnderReview`). The erasure is stuck: recorded as Approved, never performed, with no UI
path to retry.

**Live-observed (2026-06-04, dev, salt missing):** approving Ndlovu's erasure surfaced
*"PseudonymSalt is not configured. Cannot execute erasure without it."*; the request was left
`Approved`, Ndlovu's PII **intact**, and the detail page offered no recovery. After configuring the salt
and submitting a **fresh** erasure, approval ran cleanly → `Completed` and Ndlovu's PII was pseudonymised
(name/email cleared, `UserName` → `deleted_user_<hash>`, account locked, row/Id retained for audit FKs).

A data subject's erasure can therefore silently end up "Approved but not performed."

## Fix options (recommend a + b)

- **(a)** Validate `PseudonymSalt` (and any other erasure prerequisites) **before** allowing Approve of
  an Erasure — fail fast, leave the request `Submitted` with a clear message.
- **(b)** Wrap the status transition + erasure execution in a single unit of work so a throw **rolls
  back** the Approve (request stays actionable).
- **(c)** (defensive) add a "retry/execute erasure" action for any `Approved` erasure request whose
  execution did not complete.

Add a handler test: erasure approve with executor throwing → request remains actionable, data untouched.

## Dev-config note

The dev environment was **missing `Wombat:PseudonymSalt`**. Set via user-secrets on `Wombat.Web`
(value recorded in `pwd_DO_NOT_COMMIT.txt`). **Production must configure `Wombat:PseudonymSalt`** (env/
secret) or all erasures fail. Consider a startup warning when it is absent.
