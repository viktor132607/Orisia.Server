FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.sln .
COPY Directory.Packages.props .
COPY global.json .
COPY Orisia.Server.API/*.csproj ./Orisia.Server.API/
COPY Orisia.Server.Data/*.csproj ./Orisia.Server.Data/
COPY Orisia.Server.Domain/*.csproj ./Orisia.Server.Domain/
COPY Orisia.Server.Core/*.csproj ./Orisia.Server.Core/
COPY Orisia.Server.Common/*.csproj ./Orisia.Server.Common/
COPY Orisia.Server.Tests.Unit/*.csproj ./Orisia.Server.Tests.Unit/


RUN dotnet restore Orisia.Server.sln

COPY . .
RUN dotnet publish Orisia.Server.API/Orisia.Server.API.csproj -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl libgssapi-krb5-2 \
    && install -d /usr/share/postgresql-common/pgdg \
    && curl --fail --show-error --silent https://www.postgresql.org/media/keys/ACCC4CF8.asc \
        -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc \
    && echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt noble-pgdg main" \
        > /etc/apt/sources.list.d/pgdg.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client-14 postgresql-client-15 postgresql-client-16 postgresql-client-17 postgresql-client-18 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Orisia.Server.API.dll"]
