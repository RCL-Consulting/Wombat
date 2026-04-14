using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using Wombat.Api.Endpoints;
using Wombat.Domain.Institutions;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// Verifies that the Clean Architecture layer boundaries defined in ARCHITECTURE.md are not violated.
/// Any failing test here represents a structural regression that must be fixed at the cause,
/// not by weakening or removing the test.
/// </summary>
public class LayerTests
{
    private static readonly Assembly DomainAssembly = typeof(Institution).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Wombat.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Wombat.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebAssembly = typeof(IScopedSender).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MsfRespondEndpoint).Assembly;

    // ─── Domain ──────────────────────────────────────────────────────────────

    [Fact]
    public void Domain_should_not_depend_on_EF()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference EF Core. Offending types: {Join(result.FailingTypeNames)}");
    }

    [Fact]
    public void Domain_should_not_depend_on_MediatR()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference MediatR. Offending types: {Join(result.FailingTypeNames)}");
    }

    [Fact]
    public void Domain_should_not_depend_on_AspNetCore()
    {
        var result = Types
            .InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Domain must not reference ASP.NET Core. Offending types: {Join(result.FailingTypeNames)}");
    }

    // ─── Application ─────────────────────────────────────────────────────────

    [Fact]
    public void Application_should_not_depend_on_Infrastructure()
    {
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Wombat.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Application must not reference Infrastructure. Offending types: {Join(result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_should_not_depend_on_AspNetCore()
    {
        var result = Types
            .InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Application must not reference ASP.NET Core. Offending types: {Join(result.FailingTypeNames)}");
    }

    [Fact]
    public void Application_handlers_should_not_accept_DbContext_directly()
    {
        // Handlers may use IApplicationDbContext, never the concrete DbContext.
        var dbContextType = typeof(DbContext);

        var offenders = ApplicationAssembly.GetTypes()
            .Where(t => !t.IsAbstract && t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Where(t => t.GetConstructors()
                .Any(ctor => ctor.GetParameters()
                    .Any(p => dbContextType.IsAssignableFrom(p.ParameterType))))
            .Select(t => t.FullName!)
            .ToList();

        offenders.Should().BeEmpty(
            because: "Application handlers must depend on IApplicationDbContext, never on DbContext directly.");
    }

    // ─── Infrastructure ───────────────────────────────────────────────────────

    [Fact]
    public void Infrastructure_should_not_depend_on_Api()
    {
        var result = Types
            .InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Wombat.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Infrastructure must not reference Api. Offending types: {Join(result.FailingTypeNames)}");
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_Web()
    {
        var result = Types
            .InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Wombat.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Infrastructure must not reference Web. Offending types: {Join(result.FailingTypeNames)}");
    }

    // ─── Web ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Web_components_should_not_reference_EF_directly()
    {
        // Scoped to Wombat.Web.Components — Program.cs legitimately references DbContext
        // for startup migrations. That bootstrap concern is acceptable outside the component tree.
        var result = Types
            .InAssembly(WebAssembly)
            .That()
            .ResideInNamespace("Wombat.Web.Components")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Blazor components must go through the Application layer, not EF Core directly. " +
                     $"Offending types: {Join(result.FailingTypeNames)}");
    }

    // ─── Api ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Api_should_not_reference_Domain_entities_directly()
    {
        // Api endpoints must expose Application DTOs, never raw Domain entity types.
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("Wombat.Api")
            .ShouldNot()
            .HaveDependencyOn("Wombat.Domain")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"Api must use Application DTOs, not Domain entities. " +
                     $"Offending types: {Join(result.FailingTypeNames)}");
    }

    private static string Join(IEnumerable<string>? names) =>
        names is null ? "(none)" : string.Join(", ", names);
}
