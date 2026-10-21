namespace ErCv.Web.UI;

/// <summary>Keys and names the UI shares between components, the browser store and wwwroot/js/app.js.</summary>
public static class UiConstants
{
    #region JS Interop

    // Functions of window.cvApp in wwwroot/js/app.js
    public const string JsDownloadFile = "cvApp.downloadFile";
    public const string JsCopyText = "cvApp.copyText";
    public const string JsKeepScroll = "cvApp.keepScroll";
    public const string JsFocusElement = "cvApp.focusElement";
    public const string JsRevealElement = "cvApp.revealElement";
    public const string JsGetItem = "cvApp.getItem";
    public const string JsSetItem = "cvApp.setItem";
    public const string JsRemoveItem = "cvApp.removeItem";
    public const string JsGetTheme = "cvApp.getTheme";
    public const string JsSetTheme = "cvApp.setTheme";
    public const string JsScrollToTop = "cvApp.scrollToTop";
    public const string JsRegisterShortcuts = "cvApp.registerShortcuts";
    public const string JsUnregisterShortcuts = "cvApp.unregisterShortcuts";

    #endregion

    #region Profile Sources

    public const string ProfileSourceDemo = "demo";
    public const string ProfileSourceMine = "mine";
    public const string ProfileSourceImported = "imported";

    // Labels of the stored profiles in step 2 and on the profile page; other stored profiles use their file name
    public const string SeniorSampleProfileLabel = "Senior sample profile";
    public const string CreatorProfileLabel = "Creator's profile";

    // The health check card: rendered by ProfileHealthCard, scrolled to by the profile page's check button
    public const string ProfileHealthCardId = "profile-health";

    #endregion

    #region Template Keys

    public const string TemplateHarvard = "harvard";
    public const string TemplateModern = "modern";
    public const string TemplateMinimalist = "minimalist";

    #endregion
}
