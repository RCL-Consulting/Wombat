using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Application.Features.Activities.Services;

public interface ISchemaValidator
{
    IReadOnlyList<ActivityValidationErrorDto> Validate(
        FormSchema schema,
        string dataJson,
        SchemaValidationMode mode,
        IReadOnlyCollection<string>? additionallyRequiredFieldKeys = null);
}
