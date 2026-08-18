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

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 10000

ENTRYPOINT ["dotnet", "Orisia.Server.API.dll"]
