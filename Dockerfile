FROM sitespeedio/sitespeed.io:7.7.2

WORKDIR /

RUN wget https://dl.minio.io/client/mc/release/linux-amd64/mc -O /bin/mc
RUN chmod +x /bin/mc

COPY docker/start.sh /start.sh
RUN chmod +x /start.sh
ENTRYPOINT ["/start.sh"]
VOLUME /sitespeed.io
WORKDIR /sitespeed.io