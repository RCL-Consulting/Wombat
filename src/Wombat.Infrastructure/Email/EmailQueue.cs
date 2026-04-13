using System.Threading.Channels;
using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

public sealed class EmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelWriter<EmailMessage> Writer => _channel.Writer;
    public ChannelReader<EmailMessage> Reader => _channel.Reader;
}
