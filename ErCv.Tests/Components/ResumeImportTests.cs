using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;
using ErCv.Web.UI;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class ResumeImportTests : BunitContext
{
    private readonly ToastService _toasts = new();
    private CVMaster? _imported;

    public ResumeImportTests() => Services.AddSingleton(_toasts);

    private IRenderedComponent<ResumeImport> Render() =>
        Render<ResumeImport>(p => p.Add(c => c.OnImport, (CVMaster profile) => _imported = profile));

    [Fact]
    public void Accepts_pdf_and_word_files()
    {
        var accept = Render().Find("input#profile-import-resume").GetAttribute("accept");

        Assert.Contains(".pdf", accept);
        Assert.Contains(".docx", accept);
    }

    [Fact]
    public void A_resume_is_handed_to_the_page_as_a_draft()
    {
        var cut = Render();
        var docx = ResumeDocx(new[] { "Jane Doe", "jane@doe.dev", "EXPERIENCE", "Engineer at Acme\t2020 - Present", "- Built the API" });

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(docx, "jane.docx"));

        cut.WaitForAssertion(() => Assert.Equal("Jane Doe", _imported?.Header.FullName));
        Assert.DoesNotContain(_toasts.Toasts, t => t.Kind == ToastKind.Error);
    }

    [Fact]
    public void A_file_that_is_not_a_resume_shows_why()
    {
        var cut = Render();

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("just text", "notes.txt"));

        cut.WaitForAssertion(() => Assert.Contains(_toasts.Toasts, t => t.Text == ResumeImportConstants.WrongTypeMessage));
        Assert.Null(_imported);
    }

    [Fact]
    public void A_file_over_the_limit_is_not_read()
    {
        var cut = Render();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary(new byte[ResumeImportConstants.MaxImportBytes + 1], "big.pdf"));

        cut.WaitForAssertion(() => Assert.Contains(_toasts.Toasts, t => t.Text == ResumeImporter.SizeError(ResumeImportConstants.MaxImportBytes + 1)));
        Assert.Null(_imported);
    }

    [Fact]
    public void The_limits_tooltip_takes_its_numbers_from_the_constants()
    {
        var cut = Render();

        var tooltip = cut.Find("#resume-import-limits");
        Assert.Equal("tooltip", tooltip.GetAttribute("role"));
        Assert.Contains($"{ResumeImportConstants.MaxImportMegabytes} MB", tooltip.TextContent);
        Assert.Contains($"first {ResumeImportConstants.MaxPdfPages} pages", tooltip.TextContent);
        Assert.Equal("resume-import-limits", cut.Find("#resume-import-info").GetAttribute("aria-describedby"));
        Assert.Equal("resume-import-limits", cut.Find("input#profile-import-resume").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void The_info_button_toggles_the_tooltip_and_escape_closes_it()
    {
        var cut = Render();

        cut.Find("#resume-import-info").Click();
        Assert.Contains("show", cut.Find("#resume-import-limits").ClassList);
        Assert.Equal("true", cut.Find("#resume-import-info").GetAttribute("aria-expanded"));

        cut.Find("#resume-import-info").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });
        Assert.DoesNotContain("show", cut.Find("#resume-import-limits").ClassList);
        Assert.Equal("false", cut.Find("#resume-import-info").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void A_wrong_type_is_refused_by_its_name_even_when_the_bytes_look_like_a_pdf()
    {
        var cut = Render();

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary("%PDF-1.7"u8.ToArray(), "notes.txt"));

        cut.WaitForAssertion(() => Assert.Contains(_toasts.Toasts, t => t.Text == ResumeImportConstants.WrongTypeMessage));
        Assert.Null(_imported);
    }

    [Fact]
    public void A_scanned_pdf_says_to_use_a_text_pdf_or_word()
    {
        var cut = Render();

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(ScannedPdf(), "scan.pdf"));

        cut.WaitForAssertion(() => Assert.Contains(_toasts.Toasts, t => t.Text == ResumeImportConstants.ScannedPdfMessage));
        Assert.Null(_imported);
    }
}
