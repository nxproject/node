# NXProject Processor 3.1.0 Image

FROM mcr.microsoft.com/dotnet/core/runtime:3.1

RUN apt-get update \
	&& apt install libc6 \
	&& apt-get install -y libgif-dev autoconf libtool automake build-essential gettext libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev \
	&& apt-get install -y libgdiplus \
	# cleanup
	&& apt-get autoremove \
	&& apt-get clean \
	&& rm -rf /var/lib/apt/lists/*
