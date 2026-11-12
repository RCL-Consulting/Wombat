namespace Wombat.Domain.Email;

/// <summary>What became of one mail the worker finished with (T400, Q18, K3).</summary>
public enum EmailOutcomeKind
{
    /// <summary>The mail server took it.</summary>
    Sent = 0,

    /// <summary>It was given up on: refused on its last try, or abandoned as the host stopped.</summary>
    Dropped = 1
}
