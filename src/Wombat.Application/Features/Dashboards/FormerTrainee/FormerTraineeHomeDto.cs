using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Application.Features.Dashboards.FormerTrainee;

/// <summary>
/// A former trainee's Home (T379, flow 13, S8 and S9; R3-H01, R3-H02): the one card "Your training record", led by how and
/// when the programme ended, then the record's pages, My authorisations among them only where a STAR was ever issued (F6).
/// Read by <see cref="GetFormerTraineeHomeQuery" />; My progress's "Your record:" line reads the same answer, so the two
/// cannot disagree on whether My authorisations has anything to show.
/// </summary>
/// <param name="EndedOn">The programme's last day (<c>TraineeProfile.EndedOn</c>): "… on 2026-10-26."</param>
/// <param name="Completed">
/// True when it was completed ("You completed your programme on 2026-10-26."), false when the registrar left it or the
/// account was erased ("Your programme ended on 2026-10-24.").
/// </param>
/// <param name="TrainingYearAtEnd">
/// The training year it ended in (<c>EndedProgrammeDto.TrainingYearAtEnd</c>): My progress's line, "Every period of your
/// programme, to training year 4, …". Null for a last day before the programme start.
/// </param>
/// <param name="Stars">
/// The record's STARs (<see cref="RecordStars" />): whether any was ever issued (F6), and how many stand in force as the
/// programme left them, Home's count ("Your 15 STARs in force, …").
/// </param>
public sealed record FormerTraineeHomeDto(DateOnly EndedOn, bool Completed, int? TrainingYearAtEnd, RecordStarsDto Stars);
