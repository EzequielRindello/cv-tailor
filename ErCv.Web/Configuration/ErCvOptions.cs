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
    public SessionSettings Session { get; set; } = new();

    #endregion

    #region Public Methods

    /// <summary>Reads the section and fails at startup, not on the first request, when a value is missing or invalid.</summary>
    public static ErCvOptions Load(IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ErCvOptions>()
            ?? throw new InvalidOperationException($"Missing \"{SectionName}\" section in appsettings.json");

        if (options.RateLimit.PermitLimit <= 0 || options.RateLimit.WindowSeconds <= 0)
            throw new InvalidOperationException($"{SectionName}:RateLimit values must be positive");
        if (options.Session.IdleTimeoutMinutes <= 0)
            throw new InvalidOperationException($"{SectionName}:Session:IdleTimeoutMinutes must be positive");

        return options;
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
