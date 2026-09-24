using System.Reflection;
using System.Security.Claims;
using FluentAssertions;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The shape half of the T113 read boundary: a request that names a trainee carries the caller.
/// </summary>
/// <remarks>
/// <para>
/// T101 closed this for requests that return activity data (<see cref="ActivityReadBoundaryTests" />). T113 found five
/// more that took a trainee id and nothing else, because they read other aggregates - curriculum progress, committee
/// reviews, entrustment decisions, feedback campaigns - which that test does not look at: passing another trainee's id
/// returned their data. The id is the tell, whatever the aggregate, so this keys on the id.
/// </para>
/// <para>
/// Like its sibling this is weaker than the rule it guards: carrying a principal is not using it. What it catches is the
/// whole class of "keyed on a caller-supplied trainee id and nothing else" at the signature, which is where all seven
/// went wrong. Each handler's use of the principal is pinned by its own tests.
/// </para>
/// </remarks>
public class TraineeReadBoundaryTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Wombat.Application.DependencyInjection).Assembly;

    /// <summary>
    /// The property names by which a request says which trainee it is about. A filter is included: narrowing a list to
    /// one trainee is naming them.
    /// </summary>
    private static readonly string[] TraineeIdPropertyNames = ["TraineeUserId", "SubjectUserId", "TraineeUserIdFilter"];

    [Fact]
    public void Requests_that_name_a_trainee_carry_the_caller()
    {
        var violations = ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequest)
            .Where(NamesATrainee)
            .Where(type => !CarriesClaimsPrincipal(type))
            .Select(type => type.FullName!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "a request keyed on a trainee id must take a ClaimsPrincipal, so its handler has someone to " +
                     "authorize; without one, naming another trainee returns their data. (T113)");
    }

    [Fact]
    public void Guard_the_scan_sees_the_requests_T113_fixed()
    {
        // If the scan stopped matching (a rename, an assembly move), the test above would pass by seeing nothing.
        var named = ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequest)
            .Where(NamesATrainee)
            .Select(type => type.Name)
            .ToList();

        named.Should().Contain(
        [
            "GetCurriculumProgressForTraineeQuery",
            "ListReviewsForTraineeQuery",
            "GetActiveDecisionsForTraineeQuery",
            "GetDecisionHistoryForEpaQuery",
            "ListMsfCampaignsForTraineeQuery"
        ]);
    }

    [Fact]
    public void Guard_the_scan_sees_a_request_that_filters_on_a_trainee()
    {
        ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequest)
            .Where(NamesATrainee)
            .Select(type => type.Name)
            .Should().Contain("ListEntrustmentDecisionsForAdminQuery");
    }

    private static bool NamesATrainee(Type requestType)
        => requestType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(property => property.PropertyType == typeof(string) &&
                             TraineeIdPropertyNames.Contains(property.Name, StringComparer.Ordinal));

    private static bool IsConcreteRequest(Type type)
    {
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
        {
            return false;
        }

        return type.GetInterfaces().Any(i =>
            i.Namespace == "MediatR" && (i.Name == "IRequest" || i.Name == "IRequest`1"));
    }

    private static bool CarriesClaimsPrincipal(Type requestType)
        => requestType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(property => typeof(ClaimsPrincipal).IsAssignableFrom(property.PropertyType));
}
