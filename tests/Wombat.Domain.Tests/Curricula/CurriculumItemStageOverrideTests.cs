using Wombat.Domain.Curricula;

namespace Wombat.Domain.Tests.Curricula;

public sealed class CurriculumItemStageOverrideTests
{
    [Fact]
    public void GetMinimumLevelForStage_ReturnsFlatLevel_WhenNoOverrides()
    {
        var item = new CurriculumItem { MinimumLevelOrder = 3 };

        Assert.Equal(3, item.GetMinimumLevelForStage(1));
        Assert.Equal(3, item.GetMinimumLevelForStage(null));
    }

    [Fact]
    public void GetMinimumLevelForStage_ReturnsOverride_WhenStageMatches()
    {
        var item = new CurriculumItem
        {
            MinimumLevelOrder = 3,
            MinimumLevelByStageJson = "{\"1\":2,\"2\":3,\"3\":4}"
        };

        Assert.Equal(2, item.GetMinimumLevelForStage(1));
        Assert.Equal(3, item.GetMinimumLevelForStage(2));
        Assert.Equal(4, item.GetMinimumLevelForStage(3));
    }

    [Fact]
    public void GetMinimumLevelForStage_FallsBackToFlat_WhenStageMissing()
    {
        var item = new CurriculumItem
        {
            MinimumLevelOrder = 5,
            MinimumLevelByStageJson = "{\"1\":2,\"2\":3}"
        };

        Assert.Equal(5, item.GetMinimumLevelForStage(4));
    }

    [Fact]
    public void ParseStageOverrides_SkipsInvalidEntries()
    {
        var overrides = CurriculumItem.ParseStageOverrides("{\"1\":2,\"bad\":3,\"0\":4,\"2\":99,\"3\":\"4\"}");

        Assert.Equal(2, overrides.Count);
        Assert.Equal(2, overrides[1]);
        Assert.Equal(4, overrides[3]);
    }

    [Fact]
    public void ParseStageOverrides_ReturnsEmpty_ForInvalidInput()
    {
        Assert.Empty(CurriculumItem.ParseStageOverrides("not json"));
        Assert.Empty(CurriculumItem.ParseStageOverrides(null));
        Assert.Empty(CurriculumItem.ParseStageOverrides("[1,2,3]"));
    }

    [Fact]
    public void NormalizeStageOverridesJson_OrdersAndCleans()
    {
        var normalized = CurriculumItem.NormalizeStageOverridesJson("{\"3\":4,\"1\":2,\"2\":3}");

        Assert.Equal("{\"1\":2,\"2\":3,\"3\":4}", normalized);
    }

    [Fact]
    public void NormalizeStageOverridesJson_ReturnsNull_ForEmpty()
    {
        Assert.Null(CurriculumItem.NormalizeStageOverridesJson(null));
        Assert.Null(CurriculumItem.NormalizeStageOverridesJson(string.Empty));
        Assert.Null(CurriculumItem.NormalizeStageOverridesJson("{}"));
    }
}
