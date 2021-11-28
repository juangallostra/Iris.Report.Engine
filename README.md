# Iris.Report.Engine

Convert webpages to PDF. 

Try it here: [https://irisreport-h3hvq4dmnq-nw.a.run.app/](https://irisreport-h3hvq4dmnq-nw.a.run.app/)

**Disclaimer**: Please do not rely on the following service for any of your projects, since this is a simple demo and I do not promise it will be up indefinitely. Instead, make your own service from the docker image if desired.

## Convert your files via API calls
A single API endpoint is offered for webpage to PDF conversion. The endpoint is `/api/generator/generate` and the full url `https://irisreport-h3hvq4dmnq-nw.a.run.app/api/generator/generate`

The API accepts only POST requests. The request body should be a JSON object with the following properties:

* `filename`: Desired filename for the PDF file. Note that the extension should not be provided.
* `targeturl`: URL of the website that should be converted to PDF. Both HTTP and HTTPS are supported.
* `method`: Conversion method. This should be an integer value in the range [0,2]. If the value is set to 0, the conversion will be made with the WebKit render engine. If the value is set to 1, the conversion will be made with the Blink render engine. If the value is set to 2, the conversion will be made with both methods and a zip file containing them will be returned. Any number different than 0, 1 or 2 will be interpreted as 2.

Sample request body:

```JSON
{
    "filename": "madboulder",
    "targeturl": "https://madboulder.org",
    "method": 0
}
```
## Docker image

If desired, a docker image ready to run can be obtained at [https://hub.docker.com/repository/docker/juangallostra/irisreport](https://hub.docker.com/repository/docker/juangallostra/irisreport)

To pull it, simply run:

```
docker pull juangallostra/irisreport:latest
```

And to run it:

```
docker run -p 9090:80 juangallostra/irisreport:latest
```

where you can replace `9090` by the port of your choice.

## Fork the project

If you want to modify the code provided here to better suit yout needs, there is one thing you should take into account. The WebKit and Blink Luinux and Windows binaries are not included in the repo, which only contains the source code.

In order to get them, see the links below:
* WebKit binaries for Windows and Linux: [https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/webkit](https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/webkit)
* Blink binaries: [https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/blink](https://help.syncfusion.com/file-formats/pdf/convert-html-to-pdf/blink)

The code expects to find the binaries under the `Iris.Report.Engine` folder, at the same level than `Iris.Report.Engine.csproj`. The expected names are `QtBinaries` & `QtBinariesLinux` for the folders containing both Windows and Linux WebKit binaries and `BlinkBinaries` & `BlinkBinariesLinux` for the Windows and Linux folders containing the Blink binaries.