docker build -t irisreport -f .\Dockerfile ..
docker tag irisreport:latest juangallostra/irisreport:latest
docker push juangallostra/irisreport:latest