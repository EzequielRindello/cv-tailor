using System.Text.RegularExpressions;

namespace ErCv.Web.Components.Shared;

// [GeneratedRegex] cannot be declared inside a .razor file (the regex generator does not see Razor's generated code),
// so the component's patterns live in this partial.
public partial class CVPreview
{
    #region Constants

    [GeneratedRegex("<meta[^>]*name=[\"']viewport[\"'][^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ViewportMetaTag();

    [GeneratedRegex("<head([^>]*)>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadOpenTag();

    [GeneratedRegex("</head>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadCloseTag();

    [GeneratedRegex("id=[\"']cv-preview-fix[\"']", RegexOptions.IgnoreCase)]
    private static partial Regex PreviewFixMarker();

    #endregion
}
