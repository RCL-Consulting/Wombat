using FluentAssertions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T127: the one-shot, per-activity notice <c>/activities/new</c> hands to <c>/activities/{id}</c>.
/// </summary>
public sealed class ActivityNoticesTests
{
    [Fact]
    public void Take_ReturnsThePostedNotice_AndRemovesIt()
    {
        var notices = new ActivityNotices();
        notices.Post(41, "success", "Submitted. It is now Requested.");

        notices.Take(41).Should().Be(new ActivityNotice("success", "Submitted. It is now Requested."));
        notices.Take(41).Should().BeNull("a notice is shown once");
    }

    [Fact]
    public void Notices_ArePerActivity()
    {
        var notices = new ActivityNotices();
        notices.Post(41, "success", "Draft saved. It has not been submitted.");
        notices.Post(42, "warning", "Saved as a draft, but not submitted: no.");

        notices.Take(42).Should().Be(new ActivityNotice("warning", "Saved as a draft, but not submitted: no."));
        notices.Take(41).Should().Be(new ActivityNotice("success", "Draft saved. It has not been submitted."));
        notices.Take(43).Should().BeNull("nothing was posted for it");
    }

    [Fact]
    public void ALaterPost_ReplacesOneNotYetTaken()
    {
        var notices = new ActivityNotices();
        notices.Post(41, "warning", "first");
        notices.Post(41, "success", "second");

        notices.Take(41).Should().Be(new ActivityNotice("success", "second"));
        notices.Take(41).Should().BeNull();
    }
}
