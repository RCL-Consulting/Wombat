using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Application.Features.Activities.Services;

public interface ISchemaValidator
{
    /// <param name="requiredFieldScope">
    /// In <see cref="SchemaValidationMode.Submit" />, the fields whose own <c>required</c> flag counts; null means every
    /// field. A transition declaring <c>validation: "owned"</c> passes the mover's writable set, so a trainee's submit is
    /// not held up by fields only the assessor can write (T105). <paramref name="additionallyRequiredFieldKeys" /> apply
    /// whatever the scope.
    /// </param>
    IReadOnlyList<ActivityValidationErrorDto> Validate(
        FormSchema schema,
        string dataJson,
        SchemaValidationMode mode,
        IReadOnlyCollection<string>? additionallyRequiredFieldKeys = null,
        IReadOnlySet<string>? requiredFieldScope = null);
}
