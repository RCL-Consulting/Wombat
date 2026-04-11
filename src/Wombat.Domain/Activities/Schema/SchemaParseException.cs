namespace Wombat.Domain.Activities.Schema;

public sealed class SchemaParseException : Exception
{
    public SchemaParseException(string message)
        : base(message)
    {
    }
}
