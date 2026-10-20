using System.IO.Compression;
using Microsoft.Playwright;

namespace ErCv.E2E
{
    public class DownloadTests : E2ETestBase
    {
        public DownloadTests(AppFixture app) : base(app)
        {
        }

        [Fact]
        public async Task Pdf_and_docx_downloads_are_non_empty_and_valid()
        {
            await GenerateAsync(pdf: true, word: true);

            var pdf = await Page.RunAndWaitForDownloadAsync(() => Button("Download PDF").ClickAsync());
            var pdfBytes = await ReadAsync(pdf);
            Assert.EndsWith(".pdf", pdf.SuggestedFilename);
            Assert.True(pdfBytes.Length > 2_000, $"PDF is only {pdfBytes.Length} bytes");
            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5));

            var docx = await Page.RunAndWaitForDownloadAsync(() => Button("Download DOCX").ClickAsync());
            var docxBytes = await ReadAsync(docx);
            Assert.EndsWith(".docx", docx.SuggestedFilename);
            Assert.True(docxBytes.Length > 2_000, $"DOCX is only {docxBytes.Length} bytes");

            using var zip = new ZipArchive(new MemoryStream(docxBytes));
            var entry = zip.GetEntry("word/document.xml");
            Assert.NotNull(entry);
            using var reader = new StreamReader(entry!.Open());
            Assert.Contains("rivera", await reader.ReadToEndAsync(), StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<byte[]> ReadAsync(IDownload download)
        {
            await using var stream = await download.CreateReadStreamAsync();
            using var memory = new MemoryStream();
            await stream!.CopyToAsync(memory);
            return memory.ToArray();
        }
    }
}
