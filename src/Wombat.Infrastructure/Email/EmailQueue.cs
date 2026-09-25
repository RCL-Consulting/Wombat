using System.Threading.Channels;

namespace Wombat.Infrastructure.Email;

public sealed class EmailQueue
{
    private readonly Channel<QueuedEmail> _channel = Channel.CreateUnbounded<QueuedEmail>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelWriter<QueuedEmail> Writer => _channel.Writer;
    public ChannelReader<QueuedEmail> Reader => _channel.Reader;
}
