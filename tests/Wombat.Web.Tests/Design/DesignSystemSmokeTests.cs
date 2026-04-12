using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

public sealed class DesignSystemSmokeTests : TestContext
{
    [Fact]
    public void PageHeader_RendersHeaderContainer()
    {
        var cut = RenderComponent<PageHeader>(parameters => parameters
            .Add(component => component.Title, "Title")
            .Add(component => component.Subtitle, "Subtitle"));

        cut.Find(".header-container").TextContent.Should().Contain("Title");
    }

    [Fact]
    public void DataTable_RendersClinicTable()
    {
        var items = new[] { "Alpha" };

        var cut = RenderComponent<DataTable<string>>(parameters => parameters
            .Add(component => component.Items, items)
            .Add(component => component.HeaderRow, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "tr");
                builder.OpenElement(1, "th");
                builder.AddContent(2, "Name");
                builder.CloseElement();
                builder.CloseElement();
            }))
            .Add(component => component.Row, item => builder =>
            {
                builder.OpenElement(0, "tr");
                builder.OpenElement(1, "td");
                builder.AddContent(2, item);
                builder.CloseElement();
                builder.CloseElement();
            }));

        cut.Find(".clinic-table").TextContent.Should().Contain("Alpha");
    }

    [Fact]
    public void FormField_RendersFormGroup()
    {
        var cut = RenderComponent<FormField>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.InputId, "name")
            .Add(component => component.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "input");
                builder.AddAttribute(1, "id", "name");
                builder.AddAttribute(2, "class", "form-control");
                builder.CloseElement();
            })));

        cut.Find(".form-group label").TextContent.Should().Contain("Name");
    }

    [Fact]
    public void ConfirmDialog_RendersDialog()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(component => component.Title, "Confirm")
            .Add(component => component.Body, "Are you sure?"));

        cut.Find("dialog").Should().NotBeNull();
    }

    [Fact]
    public void PagerControls_RendersPager()
    {
        var cut = RenderComponent<PagerControls>(parameters => parameters
            .Add(component => component.Page, 1)
            .Add(component => component.PageSize, 20)
            .Add(component => component.TotalCount, 42));

        cut.Find(".pager-info").TextContent.Should().Contain("42");
    }

    [Fact]
    public void StatePanel_RendersEmptyState()
    {
        var cut = RenderComponent<StatePanel>(parameters => parameters
            .Add(component => component.IsEmpty, true)
            .Add(component => component.EmptyTitle, "Nothing here")
            .Add(component => component.EmptyBody, "Try again later")
            .Add(component => component.ChildContent, (RenderFragment)(_ => { })));

        cut.Find(".detail-card--empty").TextContent.Should().Contain("Nothing here");
    }

    [Fact]
    public void Alert_RendersKindClass()
    {
        var cut = RenderComponent<Alert>(parameters => parameters
            .Add(component => component.Kind, "danger")
            .Add(component => component.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Error"))));

        cut.Find(".alert.alert-danger").TextContent.Should().Contain("Error");
    }

    [Fact]
    public void Skeleton_RendersSkeletonClass()
    {
        var cut = RenderComponent<Skeleton>(parameters => parameters
            .Add(component => component.Count, 2));

        cut.FindAll(".skeleton").Count.Should().Be(2);
    }
}
