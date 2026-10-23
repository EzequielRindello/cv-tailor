namespace ErCv.Web.Models;

/// <summary>The link to a Greenhouse or Lever job post (APP-07).</summary>
public record JobPostImportRequest(string? Link);

/// <summary>
/// A job post read from its link: the text as the corpus writes it (title, a blank line, the body), the role and
/// the company (empty when the board does not give one), and the board it came from.
/// </summary>
public record JobPostImportResponse(string Text, string Title, string Company, string Source);
