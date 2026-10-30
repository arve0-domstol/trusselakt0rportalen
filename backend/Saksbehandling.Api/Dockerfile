# Builds and runs the API for local/course use.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Saksbehandling.Api.csproj ./
RUN dotnet restore "./Saksbehandling.Api.csproj"

COPY . .
RUN dotnet publish "./Saksbehandling.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# pandoc is used to render an HTML preview of uploaded office documents.
RUN apt-get update \
    && apt-get install -y --no-install-recommends pandoc \
    && rm -rf /var/lib/apt/lists/*

# wkhtmltopdf renders the case summary PDF export. Pinned to 0.12.5 (the build
# the reporting template was developed against); its bundled Qt links libssl1.1,
# which is pulled from the Debian bullseye archive since bookworm ships libssl3.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates wget fontconfig libjpeg62-turbo libxrender1 libxext6 \
        xfonts-75dpi xfonts-base \
    && echo "deb http://deb.debian.org/debian bullseye main" > /etc/apt/sources.list.d/bullseye-compat.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends libssl1.1 \
    && wget -q https://github.com/wkhtmltopdf/wkhtmltopdf/releases/download/0.12.5/wkhtmltox_0.12.5-1.stretch_amd64.deb -O /tmp/wkhtmltox.deb \
    && (dpkg -i /tmp/wkhtmltox.deb || apt-get install -y -f --no-install-recommends) \
    && rm -f /tmp/wkhtmltox.deb /etc/apt/sources.list.d/bullseye-compat.list \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Saksbehandling.Api.dll"]
