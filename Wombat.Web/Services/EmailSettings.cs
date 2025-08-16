namespace Wombat.Web.Services
{
    public class EmailSettings
    {
        public bool UseSMTP { get; set; } = true;
        public string Host { get; set; } = "";
        public int Port { get; set; } = 465;     // cPanel: 465 (implicit TLS) works best
        public string Email { get; set; } = "";  // full mailbox, e.g. wombat@rcl.co.za
        public string Password { get; set; } = "";
        public bool EnableSSL { get; set; } = true;
    }

}
