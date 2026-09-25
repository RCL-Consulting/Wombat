using Bunit;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>
/// The base class of a bUnit test that releases work the page is awaiting, such as a held command or a gated load. It
/// holds the one timeout for a wait on that work. (T244)
/// </summary>
/// <remarks>
/// <para>
/// Most of this suite never needs it. Its fakes answer with completed tasks, so an event's handler, and the render after
/// it, run on the test's own thread before the next line of the test, and a wait passes at its first check. A test that
/// releases a task the page is awaiting is different: the page's continuation is posted to the renderer's dispatcher,
/// which runs it on a thread-pool thread, and the render the test waits for comes from there. bUnit gives a wait one
/// second by default (<see cref="TestContextBase.DefaultWaitTimeout" />), and with the Integration suite running beside
/// this one that hop has taken longer.
/// </para>
/// <para>
/// A wait that follows such a release passes <see cref="AsyncWorkTimeout" />, and no other wait does. A longer timeout
/// lets a late render pass, and a render that is late because the page started work it did not await is a defect in the
/// page. Check that the page awaits the task before giving a wait this timeout.
/// </para>
/// </remarks>
public abstract class WombatTestContext : TestContext
{
    /// <summary>
    /// How long a wait may take for a render that follows a task the test released: long enough for a thread-pool thread
    /// on a busy machine, short enough that a page that never renders fails in seconds rather than hanging the run.
    /// </summary>
    public static readonly TimeSpan AsyncWorkTimeout = TimeSpan.FromSeconds(5);
}
