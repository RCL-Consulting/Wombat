using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Channels;

namespace Wombat.Web.Services
{
    public class EmailWorker : BackgroundService
    {
        private readonly Channel<(string to, string subject, string html)> _q;
        private readonly IEmailSender _sender;
        private readonly ILogger<EmailWorker> _log;
        public EmailWorker(Channel<(string, string, string)> q, IEmailSender sender, ILogger<EmailWorker> log)
        { _q = q; _sender = sender; _log = log; }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await foreach (var (to, subject, html) in _q.Reader.ReadAllAsync(ct))
            {
                try { await _sender.SendEmailAsync(to, subject, html); }
                catch (Exception ex) { _log.LogError(ex, "Queued email failed to {To}", to); }
            }
        }
    }
}
