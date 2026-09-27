using System.Text.RegularExpressions;
using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Identity;

/// <summary>
/// T335, flow 01: a role is shown to people by its sentence-case label, never by its key ("Committee member", not
/// "CommitteeMember"). One map, beside the keys, which the shell, Home and the nominee gate's refusals share.
/// </summary>
public sealed class WombatRoleLabelsTests
{
    [Theory]
    [InlineData(WombatRoles.Administrator, "Administrator")]
    [InlineData(WombatRoles.CollegeAdmin, "College admin")]
    [InlineData(WombatRoles.InstitutionalAdmin, "Institutional admin")]
    [InlineData(WombatRoles.SpecialityAdmin, "Speciality admin")]
    [InlineData(WombatRoles.SubSpecialityAdmin, "Sub-speciality admin")]
    [InlineData(WombatRoles.CommitteeMember, "Committee member")]
    [InlineData(WombatRoles.Coordinator, "Coordinator")]
    [InlineData(WombatRoles.Assessor, "Assessor")]
    [InlineData(WombatRoles.Trainee, "Trainee")]
    [InlineData(WombatRoles.PendingTrainee, "Pending trainee")]
    public void EachRole_HasItsSentenceCaseLabel(string role, string label)
    {
        Assert.Equal(label, WombatRoleLabels.For(role));
    }

    [Fact]
    public void EveryRole_HasALabel_AndNoTwoShareOne()
    {
        Assert.Equal(WombatRoles.All.Order(StringComparer.Ordinal), WombatRoleLabels.All.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(WombatRoleLabels.All.Count, WombatRoleLabels.All.Values.Distinct(StringComparer.Ordinal).Count());
    }

    // Sentence case: a capital first letter and nothing else capitalised (mixed capitalisation was T190's symptom).
    [Fact]
    public void EveryLabel_IsSentenceCase()
    {
        foreach (var label in WombatRoleLabels.All.Values)
        {
            Assert.True(char.IsUpper(label[0]), label);
            Assert.False(Regex.IsMatch(label[1..], "[A-Z]"), $"'{label}' should be sentence case");
        }
    }

    // A key that names no role (a stored value for a role since removed, a typed address) is shown as it is, never
    // dropped: whether it is a role at all is the caller's question.
    [Theory]
    [InlineData("Registrar")]
    [InlineData("")]
    public void AKeyThatIsNoRole_IsShownAsItIs(string key)
    {
        Assert.Equal(key, WombatRoleLabels.For(key));
    }

    // Role names are ordinal, as Identity's role claims are.
    [Fact]
    public void TheKeys_AreMatchedExactly()
    {
        Assert.Equal("committeemember", WombatRoleLabels.For("committeemember"));
    }
}
