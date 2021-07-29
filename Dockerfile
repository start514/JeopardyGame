FROM ubuntu:latest

LABEL maintainer="solutionmanager0807@gmail.com"

RUN apt-get update -qq; \
apt-get install -qq -y \
zip \
&& apt-get clean \
&& rm -rf /var/lib/apt/lists/*

COPY HeadlessServer.zip .
RUN unzip HeadlessServer.zip

RUN useradd -ms /bin/bash unity
RUN chown unity:unity -R headless_server.x86_64
USER unity

ENTRYPOINT ./headless_server.x86_64