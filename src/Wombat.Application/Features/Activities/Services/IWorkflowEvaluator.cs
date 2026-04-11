using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

public interface IWorkflowEvaluator
{
    WorkflowEvaluationResult Evaluate(
        Workflow workflow,
        Activity activity,
        string transitionKey,
        ClaimsPrincipal principal);
}
