namespace NestVault.Api.Data
{
    public class MyRateLimitOptions
    {
        public bool EnableIpRateLimiting { get; set; }
        public bool EnableClientIdRateLimiting { get; set; }
        public int HttpStatusCode { get; set; }
        public string RealIpHeader { get; set; }
        public string ClientIdHeader { get; set; }
        public int TimeoutInMinutes { get; set; }
    }
}
