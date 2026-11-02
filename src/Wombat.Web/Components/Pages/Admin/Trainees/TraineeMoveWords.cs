using Wombat.Application.Features.Trainees;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Trainees;

/// <summary>
/// The words of a registrar's move to another curriculum version on their profile (T392, flow 15, boards R2-D01–D07b,
/// R3-D03; Q8, Q20, M1, M2 of round 2): the line under the Curriculum picker by the chosen version's state, the submit's
/// words while the picker differs from the pin, and the dialog that asks before the move. Every sentence names the
/// registrar, never a pronoun (M1). No count is read before the move (Q20): the built result says what the replay checked.
/// </summary>
public static class TraineeMoveWords
{
    /// <summary>The submit while the picker differs from the pinned version: it opens the move's dialog (T2, decision 6).</summary>
    public const string SaveAndMove = "Save profile and move…";

    /// <summary>The dialog's confirm, the primary (Q8).</summary>
    public const string MoveConfirm = "Move and save";

    /// <summary>A curriculum version as the picker and the sentences name it: "Paediatric EPA Curriculum (11.2)".</summary>
    public static string Label(CurriculumChoiceDto choice) => $"{choice.Name} ({choice.Version})";

    /// <summary>D05: the chosen version is the institution's active adoption for the registrar's discipline.</summary>
    public static string ActiveLine(string speciality, string subSpeciality)
        => $"The version your institution has adopted for {speciality} / {subSpeciality}.";

    /// <summary>D01: the registrar is kept on a version the institution has superseded.</summary>
    public static string KeptLine(string name, CurriculumChoiceDto pinned, DateOnly supersededOn)
        => $"{name} is on {Label(pinned)}, which your institution superseded on {TraineeProfileWords.Iso(supersededOn)}. " +
           $"Keeping it keeps {name} on {pinned.Version}.";

    /// <summary>
    /// D01's case with no day to name: a pinned version that is not the active adoption and that no later adoption followed
    /// (never adopted by the institution). NEW: no board draws it.
    /// </summary>
    public static string KeptLineNoDay(string name, CurriculumChoiceDto pinned)
        => $"{name} is on {Label(pinned)}, which is not the version your institution has adopted. " +
           $"Keeping it keeps {name} on {pinned.Version}.";

    /// <summary>D02: the picker has moved to the active adoption; nothing is saved until the dialog is confirmed.</summary>
    public static string MoveLine(string name, CurriculumChoiceDto to, CurriculumChoiceDto from)
        => $"When you save, {name} moves to {Label(to)}. {PortfolioWords.Possessive(name)} completed activities are counted " +
           $"again against {to.Version} in the same save, so {PortfolioWords.Possessive(name)} progress page reads " +
           $"{to.Version}'s targets at once. {NoWayBack(name, to, from)}";

    /// <summary>D03's title: "Move Sipho Ndlovu to Paediatric EPA Curriculum (11.2)?".</summary>
    public static string MoveTitle(string name, CurriculumChoiceDto to) => $"Move {name} to {Label(to)}?";

    /// <summary>D03's first paragraph.</summary>
    public static string MoveCounted(string name, CurriculumChoiceDto to)
        => $"{PortfolioWords.Possessive(name)} completed activities are counted again against {to.Version} as the profile " +
           $"saves, and from then on they credit {to.Version}'s items.";

    /// <summary>D02's last sentence and D03's second paragraph (M2: conditional on the adoption, so going back stays open).</summary>
    public static string NoWayBack(string name, CurriculumChoiceDto to, CurriculumChoiceDto from)
        => $"{name} cannot be moved back to {from.Version} while {to.Version} is the version your institution has adopted.";
}
