namespace ErCv.Web.Configuration;

/// <summary>
/// Hosting settings an operator may tune per deployment, bound from the "ErCv" section of appsettings.json.
/// Values that tune the CV logic itself are constants next to the code that uses them, not settings.
/// </summary>
public sealed class ErCvOptions
{
    #region Constants

    public const string SectionName = "ErCv";

    #endregion

    #region Properties

    public RateLimitSettings RateLimit { get; set; } = new();

    /// <summary>The stricter limit of the job post import endpoint (APP-07): each request makes one outbound call.</summary>
    public RateLimitSettings ImportRateLimit { get; set; } = new();

    public SessionSettings Session { get; set; } = new();

    #endregion

    #region Public Methods

    /// <summary>Reads the section and fails at startup, not on the first request, when a value is missing or invalid.</summary>
    public static ErCvOptions Load(IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ErCvOptions>()
            ?? throw new InvalidOperationException($"Missing \"{SectionName}\" section in appsettings.json");

        RequirePositive(options.RateLimit, nameof(RateLimit));
        RequirePositive(options.ImportRateLimit, nameof(ImportRateLimit));
        // A "stricter" policy configured looser is a mistake an operator can make without noticing
        if ((long)options.ImportRateLimit.PermitLimit * options.RateLimit.WindowSeconds
            > (long)options.RateLimit.PermitLimit * options.ImportRateLimit.WindowSeconds)
            throw new InvalidOperationException($"{SectionName}:ImportRateLimit must not allow more requests per second than {SectionName}:RateLimit");
        if (options.Session.IdleTimeoutMinutes <= 0)
            throw new InvalidOperationException($"{SectionName}:Session:IdleTimeoutMinutes must be positive");

        return options;
    }

    #endregion

    #region Private Methods

    private static void RequirePositive(RateLimitSettings limit, string name)
    {
        if (limit.PermitLimit <= 0 || limit.WindowSeconds <= 0)
            throw new InvalidOperationException($"{SectionName}:{name} values must be positive");
    }

    #endregion

    #region Nested Types

    public sealed class RateLimitSettings
    {
        /// <summary>API requests one client IP may make per window.</summary>
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
    }

    public sealed class SessionSettings
    {
        /// <summary>How long generated files stay downloadable after the last request.</summary>
        public int IdleTimeoutMinutes { get; set; }
    }

    #endregion
}
