namespace Wombat.Application.Features.Institutions;

/// <summary>
/// A speciality's and a sub-speciality's field refusals, in the label's words (T394, flow 16, N1, T324): the validators
/// write them and the record pages check the same before they send, so each reads the same wherever it is found.
/// </summary>
public static class SpecialityFieldMessages
{
    /// <summary>Name left empty (R3-A08).</summary>
    public const string NameRequired = "Enter a name.";

    /// <summary>A name past its 200 characters.</summary>
    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>A description past its 2,000 characters (decision 4: the heavy fixture sits at the cap).</summary>
    public const string DescriptionTooLong = "A description is at most 2,000 characters.";

    /// <summary>The longest name a speciality or a sub-speciality takes.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The longest description a speciality or a sub-speciality takes.</summary>
    public const int DescriptionMaxLength = 2000;
}
