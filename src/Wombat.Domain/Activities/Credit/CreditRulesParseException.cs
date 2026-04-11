namespace Wombat.Domain.Activities.Credit;

public sealed class CreditRulesParseException : Exception
{
    public CreditRulesParseException(string message)
        : base(message)
    {
    }
}
