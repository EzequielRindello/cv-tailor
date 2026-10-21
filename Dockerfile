FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ErCv.Web/ErCv.Web.csproj ErCv.Web/
RUN dotnet restore ErCv.Web/ErCv.Web.csproj
COPY ErCv.Web/ ErCv.Web/
RUN dotnet publish ErCv.Web/ErCv.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
# Chrome for PDF rendering. Ubuntu's "chromium" package is a snap stub that does not run in containers,
# so install the official Chrome .deb. PuppeteerSharp skips its own download when the path is set.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates wget fonts-liberation \
    && wget -q -O /tmp/chrome.deb https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb \
    && apt-get install -y --no-install-recommends /tmp/chrome.deb \
    && rm -f /tmp/chrome.deb \
    && rm -rf /var/lib/apt/lists/*
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/google-chrome-stable \
    ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build /app .
# Run as the image's built-in non-root user, so a compromised Chrome (it runs with --no-sandbox) is not root.
# Its home (/home/app) is writable, which Chrome needs for its profile and crash data.
USER $APP_UID
# Render provides PORT; Program.cs binds to it
EXPOSE 10000
ENTRYPOINT ["dotnet", "ErCv.Web.dll"]
