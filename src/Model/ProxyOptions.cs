namespace ProxyServer.Model
{
    public class ProxyOptions
    {
        public string Scheme { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public PathPrefixType PrefixType { get; set; }

        public string DefinedPrefix { get; set; }
    }
}
