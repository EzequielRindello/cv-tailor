using System.Text.RegularExpressions;

namespace ErCv.Web.Components.Pages;

// [GeneratedRegex] cannot be declared inside a .razor file (the regex generator does not see Razor's generated code),
// so the component's patterns live in this partial.
public partial class Profile
{
    #region Constants

    // A validator path such as "experience[1].skills[3]": the section, the entry index and the field
    [GeneratedRegex(@"^(\w+)(?:\[(\d+)\])?(?:\.(\w+))?")]
    private static partial Regex ProfilePath();

    // Between a lowercase and an uppercase letter: "startDate" reads as "start Date"
    [GeneratedRegex("(?<=[a-z])(?=[A-Z])")]
    private static partial Regex CamelCaseBoundary();

    #endregion
}
