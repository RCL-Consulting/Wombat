using Wombat.Domain.Epas;

namespace Wombat.Domain.MultiSourceFeedback;

/// <summary>
/// One EPA a multi-source feedback campaign is declared to be evidence for. (T121)
/// </summary>
/// <remarks>
/// <para>
/// The College settled D9 on 2026-09-20: a campaign is run <b>once per period and covers many EPAs</b>,
/// not once per EPA. Per EPA would have been 15 × 8 = 120 returned questionnaires per registrar per year
/// against a department's same ~30 consultants and nurses; response rates would collapse and nothing
/// would ever reach <c>MinimumResponses</c>. So the coverage is a set, and this row is a member of it.
/// </para>
/// <para>
/// Read that as <i>"a campaign covering EPA 7 was released this period"</i> — never as
/// <i>"a campaign about EPA 7"</i>. Nothing in the questionnaire is EPA-specific; the EPAs say what the
/// feedback is being offered as evidence for.
/// </para>
/// <para>
/// Chosen at creation from the subject's own curriculum and <b>re-validated at release</b>: a trainee may
/// be moved between curricula in between, and a release must not fail because an administrator moved
/// someone. An EPA that has left the subject's curriculum is dropped with a log line.
/// </para>
/// </remarks>
public sealed class MsfCampaignEpa
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int EpaId { get; set; }

    /// <summary>
    /// When this EPA's evidence activity was created, or null if it never was.
    /// </summary>
    /// <remarks>
    /// Declaring coverage and recording evidence are different facts, and a release can honour the
    /// first without the second: an EPA that has left the subject's curriculum by release day is
    /// dropped rather than thrown on. Without this column the campaign could only say what it
    /// DECLARED, so a committee reading "evidence for EPA 1, 2, 3, recorded as one activity each"
    /// would be told about a record that was never written.
    /// <para>
    /// <b>Not the coverage source, and read by nothing (T186).</b> Which EPAs a released campaign covered is
    /// read from the evidence rows its release wrote (<c>MsfCampaignCoverage</c>), by the committee
    /// snapshot, the coverage grid, the campaign report and the portfolio PDF alike, because a campaign
    /// released before this stamp existed has the rows and a null here. The release still writes it; the
    /// column is left for a later migration to drop.
    /// </para>
    /// </remarks>
    public DateTime? RecordedOn { get; set; }

    public MsfCampaign Campaign { get; set; } = null!;
    public Epa Epa { get; set; } = null!;
}
