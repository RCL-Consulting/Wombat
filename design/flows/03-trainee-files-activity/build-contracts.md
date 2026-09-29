# T342 step 6 — the contracts wave 2 builds on

Wave 1 is merged on `t342`: lane A1 "lists" and lane A2 "filing". Every name below is in the code; read it there before
relying on it. Namespaces are under `Wombat.Application.Features.Activities` unless given.

## From lane A1 (lists)
- **Needs you.** `Queries.ListNeedsYou.ListNeedsYouQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivitySummaryDto>>`.
  - It returns the caller's own drafts and returned work, most recently updated first.
  - Home's card and My activities both send it. Home lists some of the rows; its count is `.Count`.
  - The Trainee dashboard's old `Inbox` now means the inbox page's rows, which are empty for a registrar with no other
    role. Home's card must send `ListNeedsYouQuery` instead.
- **Row fields.** `Dtos.ActivitySummaryDto` gains init members, filled by the subject list and Needs you (the inbox leaves
  them null):
  - `ActivityHolderDto? Holder`
  - `string? NomineeName`
  - `ActivityReturnDto? Returned`
  - `bool IsReturned`
  - `string? DisplayName`
  - `bool DisplayNameHasNominee`: when true, drop the link's second line.
- **Holder.** `ActivityHolderDto(ActivityHolderKind Kind, string? UserId, string? Name, bool IsViewer, DateTime? Since)`,
  with `enum ActivityHolderKind { Author, Person, Waiting, Done, Closed }`.
  - `IsViewer` → "You" / "With you.".
  - `Waiting` → "Waiting for <CurrentStateLabel>.".
  - `Since` is UTC; format it in SAST with the zone.
- **Returned.** `ActivityReturnDto(string ByUserId, string ByName, DateTime ReturnedOn, string? Note)`, UTC.
- **Paging.** `ListActivitiesBySubjectQuery(SubjectUserId, Principal, int Page = 1, int PageSize = 20)` returns
  `ActivityListPageDto(Items, Page, PageSize, TotalCount)` with `PageCount`.
  - Page and size are clamped; a page past the end serves the last page.
  - `MyActivities.razor` shows page 1 only until the pager is built.
- **Detail.** `ActivityDetailDto` gains `Holder`, `Returned`, `NomineeName` and `DisplayName`, filled by
  `GetActivityByIdQuery`. It is still null for a missing or unreadable activity (T101).
- **Names.** `DisplayName` is "Type · EPA · date".
  - E7: " · <nominee>" is added when a sibling of the same subject shares the rest.
  - E9: "· no EPA yet · no date yet" when both are missing, "· no date" when only the date is.
  - Helper: `Services.ActivityDisplayNames`.

## From lane A2 (filing)
- **Picker.** `ActivityTypeListItemDto` gains `ActivityTypeShape Shape` (`Rated`, `LoggedByYou`,
  `DiscussedOrReviewed`) and `bool CreditsNothing`. Group headings, from the boards: "Rated by an assessor", "Discussed
  or reviewed, not rated", "Logged by you". Leave an empty group out.
- **A move's words.** `ActivityActionDto` gains `HandOffFieldKey`, `HandsToName` (null when unfilled),
  `TargetStateLabel`, `TargetIsTerminal` and `ResultSentence`.
  - Result sentences: "Submitted. It is now <Label>.", "<Label>." for a state no move leaves ("Logged.", "Cancelled."),
    or "It is now <Label>.".
  - On Log an activity there is no activity yet. Use the Domain helpers `MoveHandOff.NomineeFieldFor(workflow, schema,
    transitionKey, fromState?)` and `MoveOutcome.For(workflow, key, fromState?)`, and take the name from the nominee
    picker's options for the field's current value.
  - The button reads "<Label> to <name>" only when there is a hand-off field and it is filled. Otherwise it is the
    label alone ("Submit", "Log").
- **File it again.** `GetFileAgainSourceQuery(int SourceActivityId, ClaimsPrincipal Principal)` returns
  `FileAgainSourceDto?(SourceActivityId, ActivityTypeId, ActivityTypeKey, string DataJson, CopiedFieldKeys, bool
  EpaDropped, string DeclinedByName, DateTime DeclinedOn)`.
  - It is null unless the caller is the source's subject and the source was declined. Otherwise ignore `?from`.
  - It saves nothing.
- **Save draft.** `SaveActivityDraftCommand(int ActivityId, string ActorUserId, ClaimsPrincipal Principal, string
  DataPatchJson)` returns `ActivityDto`. It writes no history row and no credit.
  - It is allowed in any state where the caller has writable fields.
  - A field refusal throws `ActivityFieldsRefusedException` (with `FieldKeys`), shown like a move's.
  - It throws `InvalidOperationException("You cannot change this activity while it is <State>.")` when the caller has
    nothing to write in that state.
- **Programme wording.** `ProgrammeStartWording.Hint(bool readerIsSubject, DateOnly start)` and `.Refusal`. The server's
  refusal is already viewer-aware.
- **Rungs.** `EntrustmentRung(int Order, string Label, string? Description)` in `Wombat.Application.Features.Epas`,
  from `IActivityReferenceDataService.GetEntrustmentScaleRungsAsync(string? scaleKey)`. The stored value is the Order:
  5 is the rung labelled "4".
- **Refusal words.** A nominee who cannot be named: "Assessor: Mohammed Patel cannot be named as an assessor.
  Choose someone else." (neutral for every refused nominee, T342 G4) A stored nominee who is no longer offered: "<name> (not on the current list)".

## Between wave 2's lanes
**`ActivityForm.razor` is lane B's** (Log an activity and the renderer). Lane C (the activity page) renders it and must
not change it. Lane B adds exactly these parameters, and lane C passes them:
- `[Parameter] public string? LockedOwnerName { get; set; }`: the person who fills a locked section.
  - "Fatima Khumalo fills this in".
  - Null → "The assessor you name fills this in".
- `[Parameter] public bool LockedSectionsClosed { get; set; }`: a closed record, so the line reads "<name> was to fill
  this in".
- `[Parameter] public bool ReaderIsSubject { get; set; }`: picks "your programme" or "the trainee's programme".
- `[Parameter] public IReadOnlyList<EntrustmentRung>? Rungs { get; set; }`: for the read-only rung row, with the chosen
  rung's descriptor.
- The refusal summary's id is `"refusal-summary"`. Each refused field's input id is `"<fieldKey>-in"`, and the
  summary's links point at it.

The Spec's classes are rebuilt in `app.css` as `round-2-review.md` § For the build says: on the spacing scale, reusing
`.details-list--stacked`, `.stack-list` and `.detail-card--emphasis`, with the phone rules scoped to these pages. Each
lane adds its classes under its own `/* Flow 03: … (T342) */` heading in `app.css`.
