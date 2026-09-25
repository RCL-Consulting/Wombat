using Wombat.Domain.Epas;

namespace Wombat.Domain.Tests.Epas;

/// <summary>
/// T253: a <c>scale_key</c> is one of three forms, told apart by the string alone.
/// </summary>
/// <remarks>
/// The seeds write <c>seed:</c> and a seed key, the builder an id as digits, and anything else is a name. Every reader
/// (credit, the rung picker, every label, the scale's delete and rename) reads the key through <see cref="ScaleBinding" />,
/// so these cases are what they all agree on.
/// </remarks>
public sealed class ScaleBindingTests
{
    [Theory]
    [InlineData("seed:cpsa:scale:v11.1", ScaleBindingKind.SeedKey)]
    [InlineData("  seed:demo:scale:o-r  ", ScaleBindingKind.SeedKey)]
    [InlineData("12", ScaleBindingKind.Id)]
    [InlineData(" 007 ", ScaleBindingKind.Id)]
    [InlineData("CPSA Paediatric Entrustment Scale v11.1", ScaleBindingKind.Name)]
    [InlineData("Seed:cpsa:scale:v11.1", ScaleBindingKind.Name)]
    [InlineData("-1", ScaleBindingKind.Name)]
    [InlineData("99999999999", ScaleBindingKind.Name)]
    [InlineData("or_scale", ScaleBindingKind.Name)]
    public void AKeyIsReadByItsShapeAlone(string scaleKey, ScaleBindingKind kind)
        => Assert.Equal(kind, ScaleBinding.Parse(scaleKey)!.Kind);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankKeyBindsNothing(string? scaleKey)
        => Assert.Null(ScaleBinding.Parse(scaleKey));

    [Fact]
    public void EachFormBindsTheScaleItNames_AndNoOther()
    {
        var ladder = new EntrustmentScale { Id = 12, SeedKey = "cpsa:scale:v11.1", Name = "Paediatric ladder" };
        var other = new EntrustmentScale { Id = 13, Name = "O-R Scale" };

        Assert.True(ScaleBinding.Parse(" seed:cpsa:scale:v11.1 ")!.Binds(ladder));
        Assert.True(ScaleBinding.Parse("12")!.Binds(ladder));
        Assert.True(ScaleBinding.Parse("Paediatric ladder")!.Binds(ladder));

        Assert.False(ScaleBinding.Parse("seed:cpsa:scale:v11.1")!.Binds(other));
        // A scale with no seed key is bound by no seed key, the empty one included.
        Assert.False(ScaleBinding.Parse("seed:")!.Binds(other));
        Assert.False(ScaleBinding.Parse("12")!.Binds(other));
        // A name binds exactly, case and all, and a seed key without its prefix is a name.
        Assert.False(ScaleBinding.Parse("paediatric ladder")!.Binds(ladder));
        Assert.False(ScaleBinding.Parse("cpsa:scale:v11.1")!.Binds(ladder));
    }

    [Fact]
    public void ForSeedKeyWritesTheKeyASeedBindsBy()
    {
        Assert.Equal("seed:cpsa:scale:v11.1", ScaleBinding.ForSeedKey("cpsa:scale:v11.1"));
        Assert.Equal("cpsa:scale:v11.1", ScaleBinding.Parse(ScaleBinding.ForSeedKey("cpsa:scale:v11.1"))!.SeedKey);
    }

    [Theory]
    [InlineData("12", true)]
    [InlineData("seed:anything", true)]
    [InlineData("CPSA Paediatric Entrustment Scale v11.1", false)]
    [InlineData("Seed: a local ladder", false)]
    [InlineData("", false)]
    public void AReservedNameIsOneAKeyWouldNotReadAsAName(string name, bool reserved)
        => Assert.Equal(reserved, ScaleBinding.IsReservedName(name));
}
