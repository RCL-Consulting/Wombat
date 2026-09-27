using System.Security.Cryptography;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T335 flow 01, W-011: the body face is Source Sans 3, self-hosted from Adobe's unmodified release beside its licence,
/// and the type rules DESIGN.md § Typography states (T328: form controls took the browser's font, not the body's).
/// </summary>
public sealed class TypographyTests
{
    /// <summary>
    /// Adobe's release 3.052R (github.com/adobe-fonts/source-sans), <c>WOFF2/VF/</c> in its WOFF2 zip, byte for byte.
    /// The licence reserves the name "Source": a converted or subset file is a Modified Version and could not be served as
    /// "Source Sans 3", so a changed file fails here rather than ship under the name.
    /// </summary>
    private static readonly Dictionary<string, string> UpstreamSha256 = new(StringComparer.Ordinal)
    {
        ["SourceSans3VF-Upright.ttf.woff2"] = "5f16566f7a40d39b339ad26be151fa5a1ab1f0c2574c7a2e619765584a1acbd8",
        ["SourceSans3VF-Italic.ttf.woff2"] = "b4959abc0569392f87c6c6ac612f90e3fe0104d283724189b7d8b6f61af347d3",
    };

    [Fact]
    public void TheBodyFace_IsSourceSans3_ServedFromThisOrigin_UprightAndItalic()
    {
        var faces = Stylesheet.AppCss().Rules
            .Where(rule => rule.Selector == "@font-face" && rule.Value("font-family") == "\"Source Sans 3\"")
            .ToList();

        faces.Select(face => face.Value("font-style")).Should().BeEquivalentTo(["normal", "italic"],
            "one face for upright and one for italic, so the browser never slants the upright itself");
        foreach (var face in faces)
        {
            face.Value("font-display").Should().Be("swap", "text shows in the fallback while the face loads");
            face.Value("font-weight").Should().Be("200 900", "one variable file carries every weight the release has");

            var source = face.Value("src")!;
            source.Should().MatchRegex(@"^url\(""/fonts/[^""]+\.woff2""\) format\(""woff2""\)$",
                "the CSP's font-src is 'self': a face is a same-origin woff2 under /fonts");
            var path = source[5..source.IndexOf("\")", StringComparison.Ordinal)];
            File.Exists(Stylesheet.WebFile(["wwwroot", .. path.TrimStart('/').Split('/')])).Should().BeTrue($"{path} is a file in wwwroot");
        }
    }

    [Fact]
    public void TheSourceSansFiles_AreAdobesRelease_Unmodified()
    {
        foreach (var (file, sha256) in UpstreamSha256)
        {
            var bytes = File.ReadAllBytes(Stylesheet.WebFile("wwwroot", "fonts", file));
            Convert.ToHexStringLower(SHA256.HashData(bytes)).Should().Be(sha256,
                $"{file} is Adobe's release file unchanged; a subset or converted copy may not keep the reserved name");
        }
    }

    [Fact]
    public void EveryFontFile_ShipsBesideItsLicence()
    {
        var fonts = Stylesheet.WebFile("wwwroot", "fonts");
        var licences = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SourceSans3"] = "SourceSans3-OFL.txt",
            ["fraunces"] = "Fraunces-OFL.txt",
        };

        foreach (var font in Directory.EnumerateFiles(fonts, "*.woff2").Select(Path.GetFileName))
        {
            var licence = licences.Where(entry => font!.StartsWith(entry.Key, StringComparison.Ordinal)).Select(entry => entry.Value)
                .Should().ContainSingle($"{font} is a face whose licence this test knows").Which;
            File.ReadAllText(Path.Combine(fonts, licence)).Should().Contain("SIL OPEN FONT LICENSE Version 1.1",
                $"{font} is served under the OFL, whose text must travel with it (W-011)");
        }

        File.ReadAllText(Path.Combine(fonts, "SourceSans3-OFL.txt")).Should().Contain("with Reserved Font Name 'Source'",
            "Adobe's licence, copied whole, keeps its Reserved Font Name clause");
    }

    [Fact]
    public void TheBodyStack_IsSourceSans3_ThenTheSystemFaces()
    {
        var css = Stylesheet.AppCss();

        css.Root()["--font-body"].Should().Be("\"Source Sans 3\", \"Segoe UI\", system-ui, sans-serif");
        css.Root()["--font-mono"].Should().Be("Consolas, \"Courier New\", monospace");
        var body = css.Computed("body");
        body["font-family"].Should().Be("var(--font-body)");
        body["line-height"].Should().Be("1.5");
    }

    [Theory]
    [InlineData("h1")]
    [InlineData("h2")]
    [InlineData("h3")]
    [InlineData("h4")]
    [InlineData("h5")]
    public void AHeading_IsSetTighterThanBody(string heading)
        => Stylesheet.AppCss().Computed(heading)["line-height"].Should().Be("1.25");

    [Fact]
    public void FormControls_InheritTheBodyFont()
    {
        // T328: with no rule, a textarea took the browser's monospace and a select its Arial, and a <button class="btn">
        // stood shorter than an <a class="btn"> beside it in the same header.
        var rule = Stylesheet.AppCss().Rules.Should()
            .ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selectors.Order().SequenceEqual(new[] { "button", "input", "select", "textarea" }),
                "one rule gives every control the body's font").Which;

        rule.Value("font").Should().Be("inherit", "the whole font: family, size and line height");
    }

    [Fact]
    public void ATable_SetsItsFiguresInColumns()
        => Stylesheet.AppCss().Computed(".clinic-table")["font-variant-numeric"].Should().Be("tabular-nums");

    [Theory]
    [InlineData(".font-mono")]
    [InlineData(".code-block")]
    public void Code_IsTheMonoStack(string selector)
        => Stylesheet.AppCss().Computed(selector)["font-family"].Should().Be("var(--font-mono)");
}
