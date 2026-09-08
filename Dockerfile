FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Orisia.Server/Directory.Packages.props ./
COPY Orisia.Server/global.json ./
COPY Orisia.Server/Orisia.Server.API/Orisia.Server.API.csproj Orisia.Server.API/
COPY Orisia.Server/Orisia.Server.Data/Orisia.Server.Data.csproj Orisia.Server.Data/
COPY Orisia.Server/Orisia.Server.Domain/Orisia.Server.Domain.csproj Orisia.Server.Domain/
COPY Orisia.Server/Orisia.Server.Core/Orisia.Server.Core.csproj Orisia.Server.Core/
COPY Orisia.Server/Orisia.Server.Common/Orisia.Server.Common.csproj Orisia.Server.Common/

RUN dotnet restore Orisia.Server.API/Orisia.Server.API.csproj

COPY Orisia.Server/. ./
RUN dotnet publish Orisia.Server.API/Orisia.Server.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orisia.Server.API.dll"]
