`localhost:5000/pulsar-secured:2.8.0` was a custom image with SSL certs that I created, and below is how you can create same:
- navigate to the directory housing the `Dockerfile` i.e. `latest-version-image`
- edit the `Dockerfile` file, and replace the pulsar image with your choice
- from the `CLI` `cd` into `latest-version-image` directory housing the `Dockerfile`
- run: `docker build . -f Dockerfile -t localhost:5000/pulsar-secured:2.8.0`
- If the previous step was successful, `cd` into the compose directory's sub-directory(tls, simple, multi) of your choice
- run: `docker compose up` to deploy pulsar

# Edit Host File
Add `pulsar1` and `proxy1` to host file. The easiest and fastest way is to use [Hostsman](https://github.com/portapps/hostsman-portable)