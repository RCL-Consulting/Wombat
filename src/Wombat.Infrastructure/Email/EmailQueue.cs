using System.Threading.Channels;

namespace Wombat.Infrastructure.Email;

/// <summary>The mails this host has queued and its worker has not yet taken (T012).</summary>
/// <remarks>
/// Unbounded, with the default options: the single-reader channel cannot count what it holds
/// (<see cref="ChannelReader{T}.CanCount" /> is false), and Home's Mail line reads this host's queue depth from it
/// (<see cref="EmailHealth" />, T400, flow 18, Q2). One worker still reads it.
/// </remarks>
public sealed class EmailQueue
{
    private readonly Channel<QueuedEmail> _channel = Channel.CreateUnbounded<QueuedEmail>();

    public ChannelWriter<QueuedEmail> Writer => _channel.Writer;
    public ChannelReader<QueuedEmail> Reader => _channel.Reader;
}
