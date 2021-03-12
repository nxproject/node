# NXProject Processor 3.1.0 Image

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
LABEL {proj_label}

RUN apt-get update \
	# && apt-get install -y autoconf libtool automake build-essential gettext \
	&& apt-get install -y --no-install-recommends libc6 libgif-dev libglib2.0-dev libcairo2-dev libtiff-dev libexif-dev libgdiplus \
	
	# cleanup
	&& apt-get autoremove \
	&& apt-get clean \
	&& rm -rf /var/lib/apt/lists/*
