namespace TaskManagement.Application.Payloads.Models
{
    /// <summary>
    /// Configuration settings for JWT token generation and validation
    /// </summary>
    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        public bool RequireHttpsMetadata { get; set; } = true;
        public int ClockSkewMinutes { get; set; } = 5;
    }
}