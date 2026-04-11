using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;

namespace Wombat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IInvitationTokenService, InvitationTokenService>();

        return services;
    }
}
