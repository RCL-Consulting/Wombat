using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Audit;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;

namespace Wombat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AuditPipelineBehavior must be first — it is the outermost wrapper.
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(AuditPipelineBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IInvitationTokenService, InvitationTokenService>();
        services.AddScoped<IMsfAggregationService, MsfAggregationService>();

        return services;
    }
}
