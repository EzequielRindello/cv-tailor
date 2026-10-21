namespace ErCv.Web.UI;

/// <summary>Keys and names the UI shares between components, the browser store and wwwroot/js/app.js.</summary>
public static class UiConstants
{
    #region JS Interop

    // Functions of window.cvApp in wwwroot/js/app.js
    public const string JsDownloadFile = "cvApp.downloadFile";
    public const string JsFocusElement = "cvApp.focusElement";
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

    #endregion

    #region Template Keys

    public const string TemplateHarvard = "harvard";
    public const string TemplateModern = "modern";
    public const string TemplateMinimalist = "minimalist";

    #endregion
}
