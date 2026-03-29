namespace CassMach.Infrastructure.Configuration
{
    public class ClaudeApiSettings
    {
        public string ApiKey { get; set; }
        public string Model { get; set; } = "claude-sonnet-4-20250514";
        public int MaxConcurrentRequests { get; set; } = 5;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
