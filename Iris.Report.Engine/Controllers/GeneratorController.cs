﻿using Iris.Report.Engine.Pages;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace Iris.Report.Engine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneratorController : Controller
    {
        private readonly IWebHostEnvironment environment;
        public GeneratorController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        [HttpPost("generate")]
        public IActionResult Generate(
            [FromBody] GenerationRequestBody generationRequestBody)
        {
            string path = "";
            switch (ToEnum(generationRequestBody.Method))
            {
                case RenderMethodEnum.Webkit:
                    path = GeneratePDFWebKit(generationRequestBody);
                    break;
                case RenderMethodEnum.Blink:
                    path = GeneratePDFBlink(generationRequestBody);
                    break;
                case RenderMethodEnum.Both:
                    GeneratePDFWebKit(generationRequestBody);
                    path = GeneratePDFBlink(generationRequestBody);
                    break;
            }

            var stream = new FileStream(path, FileMode.Open);
            var result = new FileStreamResult(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            result.FileDownloadName = $"{generationRequestBody.Filename}.pdf";
            return result;
        }

        private string GeneratePDFWebKit(GenerationRequestBody body)
        {
            HtmlToPdfConverter WebKitHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            WebKitConverterSettings WebKitSettings = new WebKitConverterSettings();
            WebKitSettings.WebKitPath = Path.Combine(environment.ContentRootPath, "QtBinariesLinux");
            //WebKitSettings.WebKitPath = Path.Combine(environment.ContentRootPath, "QtBinaries");
            //WebKitSettings.WebKitPath = ".\\QtBinaries";
            WebKitHtmlConverter.ConverterSettings = WebKitSettings;
            return RenderPDF(WebKitHtmlConverter, body.TargetUrl, body.Filename);
        }

        private string GeneratePDFBlink(GenerationRequestBody body)
        {
            //Initialize HTML to PDF converter 
            HtmlToPdfConverter BlinkHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            BlinkConverterSettings BlinkSettings = new BlinkConverterSettings();

            BlinkSettings.BlinkPath = Path.Combine(environment.ContentRootPath, "BlinkBinariesLinux");
            //BlinkSettings.BlinkPath = Path.Combine(environment.ContentRootPath, "BlinkBinaries");
            //BlinkSettings.BlinkPath = ".\\BlinkBinaries";

            //Set command line arguments to run without sandbox.
            BlinkSettings.CommandLineArguments.Add("--no-sandbox");
            BlinkSettings.CommandLineArguments.Add("--disable-setuid-sandbox");

            //Assign WebKit settings to HTML converter
            BlinkHtmlConverter.ConverterSettings = BlinkSettings;

            return RenderPDF(BlinkHtmlConverter, body.TargetUrl, body.Filename);

        }

        private string RenderPDF(HtmlToPdfConverter converter, string url, string filename)
        {
            //Convert URL to PDF
            PdfDocument document = converter.Convert(url);

            string path = Path.Combine(environment.WebRootPath,
                                         "output",
                                         $"{filename}.pdf");

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

    }

    public class GenerationRequestBody
    {
        public string Filename { get; set; } = default!;
        public string TargetUrl { get; set; } = default!;
        public int Method { get; set; }
    }
}