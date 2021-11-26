#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# Blink dependencies
RUN apt-get update && \
    apt-get install -yq --no-install-recommends \ 
    libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 \ 
    libexpat1 libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 libgtk-3-0 libnspr4 \ 
    libpango-1.0-0 libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 \ 
    libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 libxtst6 \ 
    libnss3

# WebKit dependencies  
RUN apt-get update && \
    apt-get -y install xvfb && \ 
    apt-get -y install fontconfig && \ 
    apt-get -y install libssl-dev && \
    apt-get -y install libx11-dev libx11-xcb-dev libxcb-icccm4-dev libxcb-image0-dev \
    libxcb-keysyms1-dev libxcb-randr0-dev libxcb-render-util0-dev libxcb-render0-dev \ 
    libxcb-shm0-dev libxcb-util0-dev libxcb-xfixes0-dev libxcb-xkb-dev libxcb1-dev \ 
    libxfixes-dev libxrandr-dev libxrender-dev

# libssl1.0-dev is required by QtWebKit to work with https pages
# It is no longer a default package, so some trickery is required to install it
RUN apt-get -y install gnupg
RUN echo "deb http://security.ubuntu.com/ubuntu bionic-security main" >> /etc/apt/sources.list
RUN apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3B4FE6ACC0B21F32
RUN apt-get update && apt-cache policy libssl1.0-dev
RUN apt-get -y install libssl1.0-dev


WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Iris.Report.Engine/Iris.Report.Engine.csproj", "Iris.Report.Engine/"]
WORKDIR /src
RUN dotnet restore "Iris.Report.Engine/Iris.Report.Engine.csproj"
COPY . .
WORKDIR "/src/Iris.Report.Engine"
RUN dotnet build "Iris.Report.Engine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Iris.Report.Engine.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Iris.Report.Engine.dll"]