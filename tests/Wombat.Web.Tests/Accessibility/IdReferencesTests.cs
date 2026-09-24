using Bunit;
using FluentAssertions;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// The page checks lean on <see cref="IdReferences" /> to say nothing is wrong, so each kind of breakage it
/// claims to find is shown being found here. A helper that silently returned empty would pass every page.
/// </summary>
public sealed class IdReferencesTests : TestContext
{
    [Fact]
    public void AMarkupWhoseReferencesAllResolve_HasNothingBroken()
    {
        var cut = RenderMarkup("""
            <label for="a">A</label><input id="a" />
            <label for="b">B</label><select id="b"></select>
            <fieldset aria-describedby="help"><legend>Group</legend><p id="help">Help</p></fieldset>
            """);

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ALabelForAMissingId_IsBroken()
    {
        // T147's defect: FormField's <label for="msf-epas"> over checkboxes with ids of their own.
        var cut = RenderMarkup("""<label for="msf-epas">Evidence</label><input id="msf-epa-1" type="checkbox" />""");

        IdReferences.Broken(cut).Should().ContainSingle().Which.Should().Contain("msf-epas").And.Contain("names no element");
    }

    [Fact]
    public void ALabelForAnElementThatCannotBeLabelled_IsBroken()
    {
        // Pointing FormField at a wrapper div would silence a missing-id check and still label nothing.
        var cut = RenderMarkup("""
            <label for="group">Group</label><div id="group"></div>
            <label for="token">Token</label><input id="token" type="hidden" />
            """);

        IdReferences.Broken(cut).Should().HaveCount(2)
            .And.Contain(problem => problem.Contains("\"group\"") && problem.Contains("<div>"))
            .And.Contain(problem => problem.Contains("\"token\"") && problem.Contains("<input>"));
    }

    [Fact]
    public void AnAriaReferenceToAMissingId_IsBroken()
    {
        var cut = RenderMarkup("""
            <input id="a" aria-describedby="a-help a-error" /><p id="a-help">Help</p>
            <div aria-labelledby="nothing"></div>
            """);

        IdReferences.Broken(cut).Should().HaveCount(2)
            .And.Contain(problem => problem.Contains("aria-describedby=\"a-error\""))
            .And.Contain(problem => problem.Contains("aria-labelledby=\"nothing\""));
    }

    [Fact]
    public void AnIdUsedTwice_IsBroken()
    {
        // A label names the first of the two, so the second control is silently unnamed.
        var cut = RenderMarkup("""<label for="a">A</label><input id="a" /><input id="a" />""");

        IdReferences.Broken(cut).Should().ContainSingle().Which.Should().Contain("\"a\" is used 2 times");
    }

    private IRenderedFragment RenderMarkup(string markup)
        => Render(builder => builder.AddMarkupContent(0, markup));
}
