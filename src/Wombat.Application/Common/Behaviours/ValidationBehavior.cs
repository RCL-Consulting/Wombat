using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Wombat.Application.Common.Behaviours;

/// <summary>
/// Runs every registered FluentValidation validator for a request before its handler executes.
/// Registered INSIDE <see cref="Wombat.Application.Audit.AuditPipelineBehavior{TRequest,TResponse}"/>
/// so a validation failure is still audited (the audit behaviour catches the thrown
/// <see cref="ValidationException"/> and records a failed entry).
///
/// Validators run sequentially (not <c>Task.WhenAll</c>) because a validator may issue an async query
/// against the shared scoped <c>ApplicationDbContext</c>, which EF Core forbids concurrently.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors.Where(failure => failure is not null));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
