using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Commands.CreateActivity;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Commands.UpdateActivityDraft;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

public sealed class ActivityHandlersTests
{
    [Fact]
    public async Task CreateUpdateTransition_FlowPersistsStateTransitionAndProgress()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        SeedRuntimeData(dbContext);

        var activityService = new ActivityService(
            dbContext,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(dbContext));

        IRequestHandler<CreateActivityCommand, Wombat.Application.Features.Activities.Dtos.ActivityDto> createHandler =
            new CreateActivityCommandHandler(activityService);
        IRequestHandler<UpdateActivityDraftCommand, Wombat.Application.Features.Activities.Dtos.ActivityDto> updateHandler =
            new UpdateActivityDraftCommandHandler(activityService);
        IRequestHandler<TransitionActivityCommand, Wombat.Application.Features.Activities.Dtos.ActivityDto> transitionHandler =
            new TransitionActivityCommandHandler(activityService);

        var subjectPrincipal = CreatePrincipal("trainee-1");
        var assessorPrincipal = CreatePrincipal("assessor-1", roles: ["Assessor"], institutionId: 10);

        var created = await createHandler.Handle(
            new CreateActivityCommand(
                200,
                "trainee-1",
                "trainee-1",
                """{ "title": "Initial draft", "epa_id": 5000 }""",
                subjectPrincipal),
            CancellationToken.None);

        var updated = await updateHandler.Handle(
            new UpdateActivityDraftCommand(
                created.Id,
                "trainee-1",
                """{ "title": "Updated draft", "epa_id": 5000, "score": 4 }""",
                subjectPrincipal),
            CancellationToken.None);

        var transitioned = await transitionHandler.Handle(
            new TransitionActivityCommand(
                updated.Id,
                "complete",
                "assessor-1",
                assessorPrincipal,
                null,
                "Looks good"),
            CancellationToken.None);

        transitioned.CurrentState.Should().Be("completed");
        transitioned.Transitions.Should().ContainSingle(transition => transition.TransitionKey == "complete");

        var persisted = await dbContext.Activities.Include(activity => activity.Transitions).SingleAsync();
        persisted.CurrentState.Should().Be("completed");
        persisted.Transitions.Should().Contain(transition => transition.TransitionKey == "complete");

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.LastActivityId.Should().Be(persisted.Id);
    }

    private static void SeedRuntimeData(ApplicationDbContext dbContext)
    {
        dbContext.Epas.Add(new Epa
        {
            Id = 5000,
            Code = "EPA-1",
            Title = "Take a history"
        });

        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        dbContext.ActivityTypes.Add(new ActivityType
        {
            Id = 200,
            Key = "hello_world",
            Name = "Hello World",
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            SchemaJson = """
                {
                  "version": 1,
                  "sections": [
                    {
                      "key": "details",
                      "title": "Details",
                      "fields": [
                        { "key": "title", "type": "text", "label": "Title", "required": true },
                        { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                        {
                          "key": "score",
                          "type": "number",
                          "label": "Score",
                          "required": true,
                          "validation": { "min": 1, "max": 5 }
                        }
                      ]
                    }
                  ]
                }
                """,
            WorkflowJson = """
                {
                  "version": 1,
                  "initial_state": "draft",
                  "states": [
                    { "key": "draft", "label": "Draft" },
                    { "key": "completed", "label": "Completed", "terminal": true }
                  ],
                  "transitions": [
                    {
                      "key": "complete",
                      "from": "draft",
                      "to": "completed",
                      "actor": "role:Assessor+scope:institution",
                      "requires_note": true,
                      "requires_fields": ["title", "epa_id", "score"]
                    }
                  ]
                }
                """,
            CreditRulesJson = """
                {
                  "counts_for": [
                    {
                      "curriculum_item_match": { "epa_field": "epa_id" },
                      "amount": 1,
                      "minimum_level_field": "score"
                    }
                  ]
                }
                """,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        });

        dbContext.SaveChanges();
    }

    private static ClaimsPrincipal CreatePrincipal(
        string userId,
        IReadOnlyCollection<string>? roles = null,
        int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
