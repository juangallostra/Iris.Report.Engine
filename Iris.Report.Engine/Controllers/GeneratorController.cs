using Iris.Report.Engine.Pages;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Iris.Report.Engine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private WebKitConverterSettings WebKitSettings { get; set; }
        private BlinkConverterSettings BlinkSettings { get; set; }

        private const string BLINK = "_blink"; 
        private const string WEBKIT = "_webkit";
        public GeneratorController(IWebHostEnvironment environment)
        {
            this.environment = environment;
            WebKitSettings = new WebKitConverterSettings();
            BlinkSettings = new BlinkConverterSettings();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                WebKitSettings.WebKitPath = Path.Combine(environment.ContentRootPath, "QtBinariesLinux");
                BlinkSettings.BlinkPath = Path.Combine(environment.ContentRootPath, "BlinkBinariesLinux");
                //Set command line arguments to run without sandbox.
                BlinkSettings.CommandLineArguments.Add("--no-sandbox");
                BlinkSettings.CommandLineArguments.Add("--disable-setuid-sandbox");

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WebKitSettings.WebKitPath = ".\\QtBinaries";
                BlinkSettings.BlinkPath = ".\\BlinkBinaries";
            }
        }

        [HttpPost("generate")]
        public IActionResult Generate(
                [FromBody] GenerationRequestBody generationRequestBody)
        {
            string path = "";
            string? secondPath = null;
            switch (ToEnum(generationRequestBody.Method))
            {
                case RenderMethodEnum.Webkit:
                    path = GeneratePDFWebKit(generationRequestBody);
                    break;
                case RenderMethodEnum.Blink:
                    path = GeneratePDFBlink(generationRequestBody);
                    break;
                case RenderMethodEnum.Both:
                    path =GeneratePDFWebKit(generationRequestBody, generationRequestBody.Filename + WEBKIT);
                    secondPath = GeneratePDFBlink(generationRequestBody, generationRequestBody.Filename + BLINK);
                    break;
            }

            if (string.IsNullOrEmpty(secondPath))
            {
                var stream = new FileStream(path, FileMode.Open);
                var result = new FileStreamResult(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
                result.FileDownloadName = $"{generationRequestBody.Filename}.pdf";
                return result;
            }
            else
            {
                return GetZippedFiles(
                    path,
                    generationRequestBody.Filename + WEBKIT,
                    secondPath,
                    generationRequestBody.Filename + BLINK);
            }
        }

        private string GeneratePDFWebKit(GenerationRequestBody body, string? outputFileName = null)
        {
            HtmlToPdfConverter WebKitHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            WebKitHtmlConverter.ConverterSettings = WebKitSettings;
            return RenderPDF(WebKitHtmlConverter, body.TargetUrl, body.Filename, outputFileName);
        }

        private string GeneratePDFBlink(GenerationRequestBody body, string? outputFileName = null)
        {
            //Initialize HTML to PDF converter 
            HtmlToPdfConverter BlinkHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            //Assign WebKit settings to HTML converter
            BlinkHtmlConverter.ConverterSettings = BlinkSettings;
            return RenderPDF(BlinkHtmlConverter, body.TargetUrl, body.Filename, outputFileName);
        }

        private string RenderPDF(HtmlToPdfConverter converter, string url, string filename, string? outputFileName = null)
        {
            //Convert URL to PDF
            PdfDocument document = converter.Convert(url);

            string path = Path.Combine(environment.WebRootPath,
                                         "output",
                                         $"{(string.IsNullOrEmpty(outputFileName) ? filename : outputFileName)}.pdf");

            var test = new FileStream(path, FileMode.Create);
            document.Save(test);

            document.Close(true);
            test.Close();
            return path;
        }
        private RenderMethodEnum ToEnum(int method)
        {
            if (method == 0) return RenderMethodEnum.Webkit;
            if (method == 1) return RenderMethodEnum.Blink;
            else return RenderMethodEnum.Both;
        }

        private FileContentResult GetZippedFiles(string path1, string filename1, string path2, string filename2)
        {
            using (var ms = new MemoryStream())
            {
                using (var archive =
                    new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    byte[] bytes1 = System.IO.File.ReadAllBytes(path1); ;
                    byte[] bytes2 = System.IO.File.ReadAllBytes(path2); ;

                    var zipEntry = archive.CreateEntry($"{filename1}.pdf",
                        CompressionLevel.Fastest);
                    using (var zipStream = zipEntry.Open())
                    {
                        zipStream.Write(bytes1, 0, bytes1.Length);
                    }

                    var zipEntry2 = archive.CreateEntry($"{filename2}.pdf",
                        CompressionLevel.Fastest);
                    using (var zipStream = zipEntry2.Open())
                    {
                        zipStream.Write(bytes2, 0, bytes2.Length);
                    }
                }
                return File(ms.ToArray(), "application/zip", "pages.zip");
            }
        }

    }

    public class GenerationRequestBody
    {
        public string Filename { get; set; } = default!;
        public string TargetUrl { get; set; } = default!;
        public int Method { get; set; }
    }
}
