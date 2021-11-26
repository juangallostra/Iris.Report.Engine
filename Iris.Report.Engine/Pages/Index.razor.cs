using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System.ComponentModel.DataAnnotations;

namespace Iris.Report.Engine.Pages
{
    public class SimpleModel
    {
        [Required]
        public string TargetUrl { get; set; } = default!;
        [Required]
        public string Filename { get; set; } = default!;
        public RenderMethodEnum RenderMethodId { get; set; }
    }
    public class RenderMethod
    {
        public string Method { get; set; } = default!;
        public RenderMethodEnum Id { get; set; }
    }

    public enum RenderMethodEnum
    {
        Webkit,
        Blink,
        Both
    }
    partial class Index
    {
        public const string WEBKIT = "_webkit";
        public const string BLINK = "_blink";

        [Inject]
        private IWebHostEnvironment environment { get; set; } = default!;
        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        private List<RenderMethod> RenderMethods => new List<RenderMethod>
        {
            new RenderMethod { Method = "WebKit", Id = RenderMethodEnum.Webkit },
            new RenderMethod { Method = "Blink", Id = RenderMethodEnum.Blink },
            new RenderMethod { Method = "Both", Id = RenderMethodEnum.Both }
        };
        private SimpleModel Model { get; set; } = default!;

        private bool isCompleted = false;

        protected override void OnParametersSet()
        {
            isCompleted = false;
            Model = new SimpleModel();
            base.OnParametersSet();
        }

        private void HandleValidSubmit(SimpleModel Model)
        {
            isCompleted = false;

            //Initialize HTML to PDF converter 
            HtmlToPdfConverter WebKitHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            HtmlToPdfConverter BlinkHtmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);

            WebKitConverterSettings WebKitSettings = new WebKitConverterSettings();
            BlinkConverterSettings BlinkSettings = new BlinkConverterSettings();

            //Set WebKit path
            //WebKitSettings.WebKitPath = ".\\QtBinaries";
            WebKitSettings.WebKitPath = Path.Combine(environment.ContentRootPath, "QtBinariesLinux");
            //BlinkSettings.BlinkPath = ".\\BlinkBinaries";
            BlinkSettings.BlinkPath = Path.Combine(environment.ContentRootPath, "BlinkBinariesLinux");
            //Set command line arguments to run without sandbox.
            BlinkSettings.CommandLineArguments.Add("--no-sandbox");
            BlinkSettings.CommandLineArguments.Add("--disable-setuid-sandbox");


            //Assign WebKit settings to HTML converter
            WebKitHtmlConverter.ConverterSettings = WebKitSettings;
            BlinkHtmlConverter.ConverterSettings = BlinkSettings;

            switch (Model.RenderMethodId)
            {
                case RenderMethodEnum.Webkit:
                    GeneratePDF(WebKitHtmlConverter, Model.TargetUrl, Model.Filename);
                    break;
                case RenderMethodEnum.Blink:
                    GeneratePDF(BlinkHtmlConverter, Model.TargetUrl, Model.Filename);
                    break;
                case RenderMethodEnum.Both:
                    GeneratePDF(WebKitHtmlConverter, Model.TargetUrl, $"{Model.Filename}{WEBKIT}");
                    GeneratePDF(BlinkHtmlConverter, Model.TargetUrl, $"{Model.Filename}{BLINK}");
                    break;
                default:
                    break;
            }

            isCompleted = true;
        }

        private void GeneratePDF(HtmlToPdfConverter converter, string url, string filename)
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
        }

        private async Task DownloadFiles()
        {
            if (Model.RenderMethodId == RenderMethodEnum.Both)
            {
                // 2 files have been generated
                foreach (var suffix in new List<string> { WEBKIT, BLINK })
                {
                    string rendererdDocPath = Path.Combine(environment.WebRootPath,
                                                "output",
                                                $"{Model.Filename}{suffix}.pdf");

                    var docFileStream = new StreamReader(rendererdDocPath);

                    var docFileName = $"{Model.Filename}{suffix}.pdf";

                    using var docStreamRef = new DotNetStreamReference(stream: docFileStream.BaseStream);

                    await JS.InvokeVoidAsync("downloadFileFromStream", docFileName, docStreamRef);
                }

            }
            else
            {

                string path = Path.Combine(environment.WebRootPath,
                                            "output",
                                            $"{Model.Filename}.pdf");

                var fileStream = new StreamReader(path);

                var fileName = $"{Model.Filename}.pdf";

                using var streamRef = new DotNetStreamReference(stream: fileStream.BaseStream);

                await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
            }
            isCompleted = false;
        }
    }

}
