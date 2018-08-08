FROM microsoft/dotnet:2.1-sdk

RUN echo "deb http://ftp.debian.org/debian stretch-backports main" | tee /etc/apt/sources.list.d/stretch-backports.list

RUN apt-get update \
    && apt-get install -y --no-install-recommends build-essential

RUN apt-get -t stretch-backports install -y --no-install-recommends cmake

RUN mkdir -p /tmp \
    && cd /tmp \
    && git clone https://github.com/rtsisyk/msgpuck.git \
    && cd msgpuck \
    && cmake -DCMAKE_BUILD_TYPE=Release . \
    && make \
    && make install \
    && rm -rf /tmp/msgpuck

RUN mkdir -p /tmp \
    && cd /tmp \
    && git clone https://github.com/msgpack/msgpack-c.git \
    && cd msgpack-c \
    && cmake -DCMAKE_BUILD_TYPE=Release . \
    && make \
    && make install \
    && rm -rf /tmp/msgpack-c

WORKDIR /app
RUN mkdir -p reproduction

ADD dotnet-core-minus-regression.sln /app
ADD reproduction/reproduction.csproj /app/reproduction/

RUN dotnet restore

ADD . /app

RUN dotnet build --no-restore -c Release

RUN (cd /app/c \
    && cmake -DCMAKE_BUILD_TYPE=Release . \
    && make \
    && make install)

CMD ["dotnet", "/app/reproduction/bin/Release/netcoreapp2.1/reproduction.dll"]
