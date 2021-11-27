# Iris.Report.Engine

Convert webpages to PDF. 

Try it here: [https://irisreport-h3hvq4dmnq-nw.a.run.app/](https://irisreport-h3hvq4dmnq-nw.a.run.app/)

## Convert your files via API calls
A single API endpoint is offered for webpage to PDF conversion. The endpoint is `/api/generator/generate` and the full url `https://irisreport-h3hvq4dmnq-nw.a.run.app/api/generator/generate`

The API accepts only POST requests. The request body should be a JSON object with the following properties:

* `filename`: Desired filename for the PDF file. Note that the extension should not be provided.
* `targeturl`: URL of the website that should be converted to PDF. Both HTTP and HTTPS are supported.
* `method`: Conversion method. This should be an integer value in the range [0,2]. If the value is set to 0, the conversion will be made with the WebKit render engine. If the value is set to 1, the conversion will be made with the Blink render engine. If the value is set to 2, the conversion will be made with both methods and a zip file containing them will be returned. Any number different than 0, 1 or 2 will be interpreted as 2.

Sample body:

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
