using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T373 (flow 11, round 3 S16; build-contracts § 0): <see cref="DataTable{TItem}" /> gains <c>StackWide</c>, a table that
/// stacks from 900px down, as flows 06, 08 and 10 hand-wrote theirs (<c>clinic-table--stack clinic-table--stack-wide</c>),
/// with the explicit roles <c>Stack</c> gives. A note row is a second <c>&lt;tr&gt;</c> from the Row fragment.
/// </summary>
public sealed class DataTableStackWideTests : TestContext
{
    private static readonly IReadOnlyList<string> Rows = ["molefe@kgk.wombat.local"];

    [Fact]
    public void StackWide_WritesBothStackClasses_AndTheTablesRoles()
    {
        var cut = Render(stack: false, stackWide: true);

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"]);
        table.GetAttribute("role").Should().Be("table");
        cut.Find("thead").GetAttribute("role").Should().Be("rowgroup");
        cut.Find("tbody").GetAttribute("role").Should().Be("rowgroup");
    }

    [Fact]
    public void Stack_Alone_StacksBelow641Only_AsBefore()
    {
        var table = Render(stack: true, stackWide: false).Find("table");

        table.ClassList.Should().Contain("clinic-table--stack");
        table.ClassList.Should().NotContain("clinic-table--stack-wide");
        table.GetAttribute("role").Should().Be("table");
    }

    [Fact]
    public void Neither_LeavesAPlainTable_WithNoRoles()
    {
        var table = Render(stack: false, stackWide: false).Find("table");

        table.ClassList.Should().NotContain("clinic-table--stack");
        table.ClassList.Should().NotContain("clinic-table--stack-wide");
        table.HasAttribute("role").Should().BeFalse();
    }

    [Fact]
    public void ARowFragment_MayWriteANoteRow_UnderItsRow()
    {
        var cut = Render(stack: false, stackWide: true);

        cut.FindAll("tbody tr").Should().HaveCount(2);
        cut.Find("tbody tr.has-note").Should().NotBeNull();
        cut.Find("tbody td.history-note").GetAttribute("colspan").Should().Be("2");
    }

    private IRenderedComponent<DataTable<string>> Render(bool stack, bool stackWide)
        => RenderComponent<DataTable<string>>(parameters => parameters
            .Add(table => table.Items, Rows)
            .Add(table => table.Caption, "Active invitations")
            .Add(table => table.Stack, stack)
            .Add(table => table.StackWide, stackWide)
            .Add(table => table.HeaderRow, (RenderFragment)(builder =>
            {
                builder.AddMarkupContent(0, "<tr role=\"row\"><th scope=\"col\" role=\"columnheader\">Invitee</th><th scope=\"col\" role=\"columnheader\">Delivery</th></tr>");
            }))
            .Add(table => table.Row, (RenderFragment<string>)(email => builder =>
            {
                builder.AddMarkupContent(0, $"<tr class=\"has-note\" role=\"row\"><th scope=\"row\" role=\"rowheader\">{email}</th><td role=\"cell\">Not delivered</td></tr>");
                builder.AddMarkupContent(1, "<tr role=\"row\"><td colspan=\"2\" class=\"history-note history-note--warning\" role=\"cell\"><p>Resend emails a new link.</p></td></tr>");
            })));
}
