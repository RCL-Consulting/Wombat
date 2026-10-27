namespace Wombat.Application.Features.Trainees;

/// <summary>
/// What recording a programme's end did (T379, flow 13, F2 and M2; build-contracts § 0): both ways out of the programme,
/// <see cref="CompleteTraineeProfileCommand" /> and <see cref="DeactivateTraineeProfileCommand" />, answer with it, so the
/// profile page builds its result from what was done ("1 activity observed after that day no longer counts.") rather than
/// from a read after the save, which would find nothing left to take back.
/// </summary>
/// <param name="TakenBack">
/// How many activities observed after the last day had their credit taken back in the end's save
/// (<see cref="ProgrammeEndCredit.CountTakenBackAsync" />); zero when the end took nothing back, and the page then says
/// nothing about it.
/// </param>
public sealed record ProgrammeEndResult(int TakenBack);
