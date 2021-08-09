`localhost:5000/pulsar-secured:2.8.0` was a custom image with SSL certs that I created, and below is how you can create same:
- navigate to the directory hosting th `Dockerfile` i.e. latest-version-image
- edit the file, replace the pulsar image with your choice
- from the `CLI` cd into latest-version-image directory housing the `Dockerfile`
- run: `docker build . -f Dockerfile -t localhost:5000/pulsar-secured:2.8.0`
- cd into the `compose` sub-directory(tls, simple, multi) of your choice
- run: `docker compose up`